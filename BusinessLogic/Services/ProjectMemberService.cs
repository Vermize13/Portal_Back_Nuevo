using Domain.Entity;
using Repository;
using Repository.Repositories;
using BusinessLogic.DTOs;

namespace BusinessLogic.Services
{
    public interface IProjectMemberService
    {
        Task AssignUserToProjectAsync(AssignUserToProjectDto dto, Guid? actorId = null);
        Task RemoveUserFromProjectAsync(Guid projectId, Guid userId, Guid? actorId = null);
        Task UpdateProjectMemberRoleAsync(Guid projectId, Guid userId, Guid roleId, Guid? actorId = null);
        Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId);
        Task<IEnumerable<ProjectMember>> GetUserProjectsAsync(Guid userId);
    }

    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public ProjectMemberService(
            IProjectMemberRepository projectMemberRepository,
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _projectMemberRepository = projectMemberRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task AssignUserToProjectAsync(AssignUserToProjectDto dto, Guid? actorId = null)
        {
            // Validate user exists
            var user = await _userRepository.GetAsync(dto.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Validate project exists
            var project = await _projectRepository.GetAsync(dto.ProjectId);
            if (project == null)
            {
                throw new InvalidOperationException("Project not found");
            }

            // Validate role exists
            var role = await _roleRepository.GetAsync(dto.RoleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found");
            }

            // Check if already assigned
            var existingMember = await _projectMemberRepository.GetByProjectAndUserAsync(dto.ProjectId, dto.UserId);
            if (existingMember != null)
            {
                if (existingMember.IsActive)
                {
                    throw new InvalidOperationException("User is already assigned to this project");
                }
                else
                {
                    // Reactivate the member
                    existingMember.IsActive = true;
                    existingMember.RoleId = dto.RoleId;
                    existingMember.JoinedAt = DateTimeOffset.UtcNow;
                    _projectMemberRepository.Update(existingMember);
                }
            }
            else
            {
                var projectMember = new ProjectMember
                {
                    ProjectId = dto.ProjectId,
                    UserId = dto.UserId,
                    RoleId = dto.RoleId,
                    JoinedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await _projectMemberRepository.AddAsync(projectMember);
            }

            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Assign,
                actorId,
                nameof(ProjectMember),
                null,
                new { dto.ProjectId, dto.UserId, dto.RoleId, UserName = user.Username, ProjectName = project.Name, RoleName = role.Name }
            );
        }

        public async Task RemoveUserFromProjectAsync(Guid projectId, Guid userId, Guid? actorId = null)
        {
            var projectMember = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, userId);
            if (projectMember == null)
            {
                throw new InvalidOperationException("Project member not found");
            }

            // Soft delete by deactivating
            projectMember.IsActive = false;
            _projectMemberRepository.Update(projectMember);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                nameof(ProjectMember),
                null,
                new { Action = "RemoveFromProject", ProjectId = projectId, UserId = userId }
            );
        }

        public async Task UpdateProjectMemberRoleAsync(Guid projectId, Guid userId, Guid roleId, Guid? actorId = null)
        {
            var projectMember = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, userId);
            if (projectMember == null)
            {
                throw new InvalidOperationException("Project member not found");
            }

            var role = await _roleRepository.GetAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found");
            }

            projectMember.RoleId = roleId;
            _projectMemberRepository.Update(projectMember);
            await _unitOfWork.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                actorId,
                nameof(ProjectMember),
                null,
                new { Action = "UpdateRole", ProjectId = projectId, UserId = userId, NewRoleId = roleId }
            );
        }

        public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId)
        {
            return await _projectMemberRepository.GetByProjectIdAsync(projectId);
        }

        public async Task<IEnumerable<ProjectMember>> GetUserProjectsAsync(Guid userId)
        {
            return await _projectMemberRepository.GetByUserIdAsync(userId);
        }
    }
}

using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IProjectMemberRepository
    {
        Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId);
        Task<IEnumerable<ProjectMember>> GetByUserIdAsync(Guid userId);
        Task<ProjectMember?> GetByProjectAndUserAsync(Guid projectId, Guid userId);
        Task AddAsync(ProjectMember projectMember);
        void Update(ProjectMember projectMember);
        void Remove(ProjectMember projectMember);
        Task<bool> ExistsAsync(Guid projectId, Guid userId);
    }

    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly BugMgrDbContext _context;

        public ProjectMemberRepository(BugMgrDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Role)
                .Where(pm => pm.ProjectId == projectId && pm.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectMember>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Include(pm => pm.Role)
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .ToListAsync();
        }

        public Task<ProjectMember?> GetByProjectAndUserAsync(Guid projectId, Guid userId)
        {
            return _context.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Role)
                .Include(pm => pm.Project)
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public Task AddAsync(ProjectMember projectMember)
        {
            _context.ProjectMembers.Add(projectMember);
            return Task.CompletedTask;
        }

        public void Update(ProjectMember projectMember)
        {
            _context.ProjectMembers.Update(projectMember);
        }

        public void Remove(ProjectMember projectMember)
        {
            _context.ProjectMembers.Remove(projectMember);
        }

        public Task<bool> ExistsAsync(Guid projectId, Guid userId)
        {
            return _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.IsActive);
        }
    }
}

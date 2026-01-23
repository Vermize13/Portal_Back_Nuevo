using Infrastructure;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<Project?> GetByCodeAsync(string code);
        Task<Project?> GetWithMembersAsync(Guid id);
        Task<Project?> GetWithSprintsAsync(Guid id);
        Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId);
        Task AddMemberAsync(ProjectMember member);
        Task RemoveMemberAsync(Guid projectId, Guid userId);
        Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId);
        Task<IEnumerable<Incident>> GetIncidentsByProjectIdAsync(Guid projectId);
        Task<List<Guid>> GetUserProjectIdsAsync(Guid userId);
    }

    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(BugMgrDbContext context) : base(context) { }

        public Task<Project?> GetByCodeAsync(string code)
        {
            return _context.Projects.FirstOrDefaultAsync(p => p.Code == code);
        }

        public Task<Project?> GetWithMembersAsync(Guid id)
        {
            return _context.Projects
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .Include(p => p.Members)
                    .ThenInclude(m => m.Role)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<Project?> GetWithSprintsAsync(Guid id)
        {
            return _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId)
        {
            return _context.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Role)
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public async Task AddMemberAsync(ProjectMember member)
        {
            await _context.ProjectMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(Guid projectId, Guid userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
            
            if (member != null)
            {
                _context.ProjectMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId)
        {
            return await _context.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Role)
                .Where(pm => pm.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetIncidentsByProjectIdAsync(Guid projectId)
        {
            return await _context.Incidents
                .Where(i => i.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetUserProjectIdsAsync(Guid userId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
        }
    }
}

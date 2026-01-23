using Domain.Entity;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;

namespace WebApi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly BugMgrDbContext _context;

        public DashboardService(BugMgrDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get list of authorized project IDs for a user
        /// Returns empty list for admins (no filtering), list of project IDs for non-admins
        /// </summary>
        private async Task<List<Guid>> GetUserAuthorizedProjectIdsAsync(Guid userId)
        {
            // Check if user is admin
            var user =await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.Role?.Code == "Admin")
            {
                // Admins see all projects - return empty list (no filtering)
                return new List<Guid>();
            }

            // Non-admins see only their assigned projects
            return await _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
        }

        // RF4.1: Get metrics by status, priority, and severity
        public async Task<IncidentMetricsResponse> GetIncidentMetricsAsync(DashboardFilterRequest filter, Guid userId)
        {
            var query = _context.Incidents.AsQueryable();

            // Apply project authorization filtering
            var authorizedProjectIds = await GetUserAuthorizedProjectIdsAsync(userId);
            if (authorizedProjectIds.Any())
            {
                // Non-admin: filter by authorized projects
                query = query.Where(i => authorizedProjectIds.Contains(i.ProjectId));
            }
            // Admin: no filtering applied

            // Apply filters
            if (filter.ProjectId.HasValue)
                query = query.Where(i => i.ProjectId == filter.ProjectId.Value);

            if (filter.SprintId.HasValue)
                query = query.Where(i => i.SprintId == filter.SprintId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(i => i.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(i => i.CreatedAt <= filter.EndDate.Value);

            var incidents = await query.ToListAsync();

            return new IncidentMetricsResponse
            {
                ByStatus = incidents
                    .GroupBy(i => i.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                ByPriority = incidents
                    .GroupBy(i => i.Priority.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                BySeverity = incidents
                    .GroupBy(i => i.Severity.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        // RF4.2: Get opened/closed incidents by sprint
        public async Task<List<SprintIncidentsResponse>> GetSprintIncidentsAsync(Guid? projectId, Guid userId)
        {
            var query = _context.Sprints.AsQueryable();

            // Apply project authorization filtering
            var authorizedProjectIds = await GetUserAuthorizedProjectIdsAsync(userId);
            if (authorizedProjectIds.Any())
            {
                // Non-admin: filter by authorized projects
                query = query.Where(s => authorizedProjectIds.Contains(s.ProjectId));
            }
            // Admin: no additional filtering

            if (projectId.HasValue)
                query = query.Where(s => s.ProjectId == projectId.Value);

            var sprints = await query
                .Include(s => s.Project)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            var result = new List<SprintIncidentsResponse>();

            foreach (var sprint in sprints)
            {
                var openedCount = await _context.Incidents
                    .CountAsync(i => i.SprintId == sprint.Id);

                var closedCount = await _context.Incidents
                    .CountAsync(i => i.SprintId == sprint.Id && 
                                   (i.Status == IncidentStatus.Closed || i.Status == IncidentStatus.Resolved));

                result.Add(new SprintIncidentsResponse
                {
                    SprintId = sprint.Id,
                    SprintName = sprint.Name,
                    OpenedCount = openedCount,
                    ClosedCount = closedCount,
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate
                });
            }

            return result;
        }

        // RF4.3: Calculate Mean Time To Resolution (MTTR)
        public async Task<MTTRResponse> GetMTTRAsync(DashboardFilterRequest filter, Guid userId)
        {
            var query = _context.Incidents
                .Where(i => i.ClosedAt != null && 
                           (i.Status == IncidentStatus.Closed || i.Status == IncidentStatus.Resolved));

            // Apply project authorization filtering
            var authorizedProjectIds = await GetUserAuthorizedProjectIdsAsync(userId);
            if (authorizedProjectIds.Any())
            {
                // Non-admin: filter by authorized projects
                query = query.Where(i => authorizedProjectIds.Contains(i.ProjectId));
            }
            // Admin: no additional filtering

            // Apply filters
            if (filter.ProjectId.HasValue)
                query = query.Where(i => i.ProjectId == filter.ProjectId.Value);

            if (filter.SprintId.HasValue)
                query = query.Where(i => i.SprintId == filter.SprintId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(i => i.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(i => i.CreatedAt <= filter.EndDate.Value);

            var incidents = await query.ToListAsync();

            if (incidents.Count == 0)
            {
                return new MTTRResponse
                {
                    AverageMTTRHours = 0,
                    AverageMTTRDays = 0,
                    ResolvedIncidentsCount = 0
                };
            }

            var resolutionTimes = incidents
                .Where(i => i.ClosedAt.HasValue)
                .Select(i => (i.ClosedAt!.Value - i.CreatedAt).TotalHours)
                .ToList();

            var avgHours = resolutionTimes.Average();

            return new MTTRResponse
            {
                AverageMTTRHours = Math.Round(avgHours, 2),
                AverageMTTRDays = Math.Round(avgHours / 24, 2),
                ResolvedIncidentsCount = incidents.Count
            };
        }

        // RF4.4: Get incident evolution over time
        public async Task<List<IncidentEvolutionResponse>> GetIncidentEvolutionAsync(DashboardFilterRequest filter, Guid userId)
        {
            var startDate = filter.StartDate ?? DateTime.UtcNow.AddMonths(-3);
            var endDate = filter.EndDate ?? DateTime.UtcNow;

            var query = _context.Incidents.AsQueryable();

            // Apply project authorization filtering
            var authorizedProjectIds = await GetUserAuthorizedProjectIdsAsync(userId);
            if (authorizedProjectIds.Any())
            {
                // Non-admin: filter by authorized projects
                query = query.Where(i => authorizedProjectIds.Contains(i.ProjectId));
            }
            // Admin: no additional filtering

            if (filter.ProjectId.HasValue)
                query = query.Where(i => i.ProjectId == filter.ProjectId.Value);

            if (filter.SprintId.HasValue)
                query = query.Where(i => i.SprintId == filter.SprintId.Value);

            var incidents = await query
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .ToListAsync();

            var result = new List<IncidentEvolutionResponse>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var openedOnDate = incidents.Count(i => i.CreatedAt.Date == currentDate);
                var closedOnDate = incidents.Count(i => i.ClosedAt.HasValue && i.ClosedAt.Value.Date == currentDate);
                var totalOpenUntilDate = incidents.Count(i => 
                    i.CreatedAt.Date <= currentDate && 
                    (!i.ClosedAt.HasValue || i.ClosedAt.Value.Date > currentDate));

                result.Add(new IncidentEvolutionResponse
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    OpenedCount = openedOnDate,
                    ClosedCount = closedOnDate,
                    TotalOpenCount = totalOpenUntilDate
                });

                currentDate = currentDate.AddDays(1);
            }

            return result;
        }
    }
}

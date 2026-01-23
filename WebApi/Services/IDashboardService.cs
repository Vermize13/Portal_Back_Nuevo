using WebApi.DTOs;

namespace WebApi.Services
{
    public interface IDashboardService
    {
        Task<IncidentMetricsResponse> GetIncidentMetricsAsync(DashboardFilterRequest filter, Guid userId);
        Task<List<SprintIncidentsResponse>> GetSprintIncidentsAsync(Guid? projectId, Guid userId);
        Task<MTTRResponse> GetMTTRAsync(DashboardFilterRequest filter, Guid userId);
        Task<List<IncidentEvolutionResponse>> GetIncidentEvolutionAsync(DashboardFilterRequest filter, Guid userId);
    }
}

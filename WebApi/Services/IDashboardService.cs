using WebApi.DTOs;

namespace WebApi.Services
{
    public interface IDashboardService
    {
        Task<IncidentMetricsResponse> GetIncidentMetricsAsync(DashboardFilterRequest filter);
        Task<List<SprintIncidentsResponse>> GetSprintIncidentsAsync(Guid? projectId);
        Task<MTTRResponse> GetMTTRAsync(DashboardFilterRequest filter);
        Task<List<IncidentEvolutionResponse>> GetIncidentEvolutionAsync(DashboardFilterRequest filter);
    }
}

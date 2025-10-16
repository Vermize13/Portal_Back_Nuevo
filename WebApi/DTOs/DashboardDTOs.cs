namespace WebApi.DTOs
{
    // RF4.1: Metrics by status, priority, and severity
    public class IncidentMetricsResponse
    {
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public Dictionary<string, int> ByPriority { get; set; } = new();
        public Dictionary<string, int> BySeverity { get; set; } = new();
    }

    // RF4.2: Incidents opened/closed by sprint
    public class SprintIncidentsResponse
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; } = default!;
        public int OpenedCount { get; set; }
        public int ClosedCount { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    // RF4.3: MTTR calculation
    public class MTTRResponse
    {
        public double AverageMTTRHours { get; set; }
        public double AverageMTTRDays { get; set; }
        public int ResolvedIncidentsCount { get; set; }
    }

    // RF4.4: Evolution of incidents
    public class IncidentEvolutionResponse
    {
        public string Date { get; set; } = default!;
        public int OpenedCount { get; set; }
        public int ClosedCount { get; set; }
        public int TotalOpenCount { get; set; }
    }

    public class DashboardFilterRequest
    {
        public Guid? ProjectId { get; set; }
        public Guid? SprintId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

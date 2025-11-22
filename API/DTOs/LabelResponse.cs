namespace API.DTOs
{
    public class LabelResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = default!;
        public string? ColorHex { get; set; }
    }
}

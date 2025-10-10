using System;

namespace Domain.Entity
{
    public class IncidentLabel
    {
        public Guid IncidentId { get; set; }
        public Incident Incident { get; set; } = default!;
        public Guid LabelId { get; set; }
        public Label Label { get; set; } = default!;
    }
}

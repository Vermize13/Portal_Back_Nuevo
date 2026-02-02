using System.Text.Json.Serialization;

namespace Domain.Entity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AuditAction
    {
        Create = 0,
        Update = 1,
        Delete = 2,
        Login = 3,
        Logout = 4,
        Assign = 5,
        Transition = 6,
        Backup = 7,
        Restore = 8,
        Upload = 9,
        Download = 10,
        HttpRequest = 11,
        SqlCommand = 12,
        Export = 13,
        Comment = 14,
        Unknown = 99
    }
}

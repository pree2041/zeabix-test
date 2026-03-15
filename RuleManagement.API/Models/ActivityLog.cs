namespace RuleManagement.API.Models
{
    public class ActivityLog
    {
        public Guid Id { get; set; }
        public Guid RuleId { get; set; }
        public string Action { get; set; } = string.Empty; 
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

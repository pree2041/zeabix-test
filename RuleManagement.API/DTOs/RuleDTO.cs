namespace RuleManagement.API.DTOs
{
    public class RuleDTO
    {
        public Guid Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ConfigJson { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}

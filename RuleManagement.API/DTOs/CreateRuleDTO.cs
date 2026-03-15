namespace RuleManagement.API.DTOs
{
    public class CreateRuleDTO
    {
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ConfigJson { get; set; } = string.Empty;
    }
}

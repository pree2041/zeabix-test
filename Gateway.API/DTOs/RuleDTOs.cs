using System;

namespace Gateway.API.DTOs
{
    public class CreateRuleDTO
    {
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ConfigJson { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
    }

    public class UpdateRuleDTO
    {
        public Guid Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ConfigJson { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
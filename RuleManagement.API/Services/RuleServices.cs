using AutoMapper;
using Microsoft.OpenApi.Validations;
using RuleManagement.API.DTOs;
using RuleManagement.API.Models;
using RuleManagement.API.Service.Contracts;

namespace RuleManagement.API.Services
{
    public class RuleServices : IRuleService
    {
        private static readonly List<Rule> _ruleMemoryStorage = new List<Rule>();
        private static readonly List<ActivityLog> _activityLogs = new List<ActivityLog>();
        private readonly IMapper _mapper;

        public RuleServices(IMapper mapper)
        {
            _mapper = mapper;
        }

        public List<RuleDTO> GetRules()
        {
            var rule = _ruleMemoryStorage.OrderBy(r => r.Priority).ToList();
            return _mapper.Map<List<RuleDTO>>(rule);
        }

        public RuleDTO GetRuleById(Guid id)
        {
            var rule = _ruleMemoryStorage.FirstOrDefault(r => r.Id == id);
            if (rule == null) throw new KeyNotFoundException($"Rule with ID {id} not found.");
            return _mapper.Map<RuleDTO>(rule);
        }

        public RuleDTO CreateRule(CreateRuleDTO dto)
        {
            var newRule = _mapper.Map<Rule>(dto);
            newRule.Id = Guid.NewGuid(); 
            _ruleMemoryStorage.Add(newRule);
            AddActivityLog("CREATE", newRule.Id, $"Created rule: {newRule.RuleName}");

            return _mapper.Map<RuleDTO>(newRule);
        }

        public RuleDTO UpdateRule(UpdateRuleDTO dto)
        {
            var existingRule = _ruleMemoryStorage.FirstOrDefault(r => r.Id == dto.Id);
            if (existingRule is null)
                throw new KeyNotFoundException("Rule not found.");
            _mapper.Map(dto, existingRule);
            AddActivityLog("UPDATE", dto.Id, $"Updated rule: {dto.RuleName}");

            return _mapper.Map<RuleDTO>(existingRule);
        }

        public RuleDTO DeleteRule(Guid id)
        {
            var rule = _ruleMemoryStorage.FirstOrDefault(r => r.Id == id);
            if (rule != null)
            {
                _ruleMemoryStorage.Remove(rule);
                AddActivityLog("DELETE", id, $"Deleted rule: {rule.RuleName}");
            }
            return _mapper.Map<RuleDTO>(rule);
        }

        private void AddActivityLog(string action, Guid ruleId, string message)
        {
            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                RuleId = ruleId,
                Details = message,
                Timestamp = DateTime.UtcNow
            };
            _activityLogs.Add(log);
        }

        public List<ActivityLog> GetLogs() => _activityLogs.OrderByDescending(l => l.Timestamp).ToList();
    }
}

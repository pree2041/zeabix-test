using RuleManagement.API.DTOs;

namespace RuleManagement.API.Service.Contracts
{
    public interface IRuleService
    {
        public List<RuleDTO> GetRules();
        public RuleDTO GetRuleById(Guid id);
        public RuleDTO CreateRule(CreateRuleDTO createRuleDTO);
        public RuleDTO UpdateRule(UpdateRuleDTO updateRuleDTO);
        public RuleDTO DeleteRule(Guid id);
    }
}

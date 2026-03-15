using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuleManagement.API.DTOs;
using RuleManagement.API.Service.Contracts;

namespace RuleManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RulesController : ControllerBase
    {
        private readonly IRuleService _ruleService;

        public RulesController(IRuleService ruleService)
        {
            _ruleService = ruleService;
        }
        [HttpGet]
        public ActionResult<List<RuleDTO>> GetRules()
        {
            var rules = _ruleService.GetRules();
            return Ok(rules);
        }

        [HttpGet("{id}")]
        public ActionResult<RuleDTO> GetRuleById(Guid id)
        {
            try
            {
                var rule = _ruleService.GetRuleById(id);
                return Ok(rule);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Rule not found" });
            }
        }

        [HttpPost]
        public ActionResult<RuleDTO> CreateRule([FromBody] CreateRuleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newRule = _ruleService.CreateRule(dto);

            return CreatedAtAction(nameof(GetRuleById), new { id = newRule.Id }, newRule);
        }

        [HttpPut]
        public ActionResult<RuleDTO> UpdateRule([FromBody] UpdateRuleDTO dto)
        {
            try
            {
                var updatedRule = _ruleService.UpdateRule(dto);
                return Ok(updatedRule);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Rule not found" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRule(Guid id)
        {
            var deletedRule = _ruleService.DeleteRule(id);
            if (deletedRule == null) return NotFound(new { message = "Rule not found" });

            return NoContent(); 
        }
    }
}

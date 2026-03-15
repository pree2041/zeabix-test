using Microsoft.AspNetCore.Mvc;
using Gateway.API.DTOs;

namespace Gateway.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RulesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _ruleServiceUrl;

        public RulesController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _ruleServiceUrl = configuration["RuleManagementServiceUrl"] ?? "http://localhost:5209";
            _httpClient.BaseAddress = new Uri(_ruleServiceUrl);
        }

        [HttpGet]
        public async Task<IActionResult> GetRules()
        {
            var response = await _httpClient.GetAsync($"api/rules");
            var content = await response.Content.ReadAsStringAsync();
            return new ContentResult { Content = content, ContentType = "application/json", StatusCode = (int)response.StatusCode };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRuleById(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/rules/{id}");
            var content = await response.Content.ReadAsStringAsync();
            return new ContentResult { Content = content, ContentType = "application/json", StatusCode = (int)response.StatusCode };
        }

        [HttpPost]
        public async Task<IActionResult> CreateRule([FromBody] CreateRuleDTO dto)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/rules", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRule([FromBody] UpdateRuleDTO dto)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/rules", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ContentResult { Content = responseContent, ContentType = "application/json", StatusCode = (int)response.StatusCode };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRule(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/rules/{id}");
            return new StatusCodeResult((int)response.StatusCode);
        }
    }
}
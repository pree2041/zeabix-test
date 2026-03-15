using Microsoft.AspNetCore.Mvc;

namespace Gateway.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _pricingServiceUrl;

        public JobsController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _pricingServiceUrl = configuration["PricingServiceUrl"] ?? "http://localhost:5200";
            _httpClient.BaseAddress = new Uri(_pricingServiceUrl);
        }

        [HttpGet("{job_id}")]
        public async Task<IActionResult> GetJobStatus(Guid job_id)
        {
            var response = await _httpClient.GetAsync($"api/jobs/{job_id}");
            var content = await response.Content.ReadAsStringAsync();
            return new ContentResult { Content = content, ContentType = "application/json", StatusCode = (int)response.StatusCode };
        }
    }
}
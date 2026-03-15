using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Gateway.API.DTOs;

namespace Gateway.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _pricingServiceUrl;

        public QuotesController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _pricingServiceUrl = configuration["PricingServiceUrl"] ?? "http://localhost:5200";
            httpClient.BaseAddress = new Uri(_pricingServiceUrl);
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { Status = "Healthy", Service = "Gateway API" });
        }

        [HttpPost("price")]
        public async Task<IActionResult> CalculatePrice([FromBody] QuoteRequestDTO request)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/quotes/price", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ContentResult { Content = responseContent, ContentType = "application/json", StatusCode = (int)response.StatusCode };
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> SubmitBulk(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var data = await ExtractDataFromFile(file);
            if (data == null)
                return BadRequest("Invalid or unsupported file format.");

            var json = System.Text.Json.JsonSerializer.Serialize(new { Requests = data });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/quotes/bulk", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ContentResult { Content = responseContent, ContentType = "application/json", StatusCode = (int)response.StatusCode };
        }

        private async Task<IEnumerable<object>?> ExtractDataFromFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();

            if (extension == ".json")
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = await System.Text.Json.JsonSerializer.DeserializeAsync<List<QuoteRequestDTO>>(stream, options);

                    if (data == null || !data.Any() || data.Any(r => string.IsNullOrWhiteSpace(r.Area)))
                        return null;

                    return data;
                }
                catch
                {
                    return null;
                }
            }

            if (extension == ".csv")
            {
                var list = new List<QuoteRequestDTO>();
                using var reader = new StreamReader(stream);
                await reader.ReadLineAsync();

                while (await reader.ReadLineAsync() is string line)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length != 4) return null;

                    if (!double.TryParse(values[0], out var w) ||
                        !DateTime.TryParse(values[2], out var t) ||
                        !double.TryParse(values[3], out var b) ||
                        string.IsNullOrWhiteSpace(values[1]))
                    {
                        return null;
                    }

                    list.Add(new QuoteRequestDTO
                    {
                        Weight = w,
                        Area = values[1].Trim(),
                        Time = t,
                        BasePrice = b
                    });
                }
                return list.Any() ? list : null;
            }
            return null;
        }
    }
}
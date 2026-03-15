using Microsoft.AspNetCore.Mvc;
using Pricing.API.DTOs;
using Pricing.API.Service.Contracts;

namespace Pricing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly IPriceService _priceService;

        public QuotesController(IPriceService priceService)
        {
            _priceService = priceService;
        }

        [HttpPost("price")]
        public async Task<ActionResult<QuoteResponseDTO>> CalculatePrice([FromBody] QuoteRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await _priceService.CalculatePrice(request);
            return Ok(response);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<BulkQuotesResponseDTO>> SubmitBulk([FromBody] CreateBulkQuotesDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await _priceService.SubmitBulkQuotes(request);
            return Ok(response);
        }
    }
}
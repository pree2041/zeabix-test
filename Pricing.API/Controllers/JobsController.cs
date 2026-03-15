using Microsoft.AspNetCore.Mvc;
using Pricing.API.DTOs;
using Pricing.API.Service.Contracts;

namespace Pricing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IPriceService _priceService;

        public JobsController(IPriceService priceService)
        {
            _priceService = priceService;
        }

        [HttpGet("{job_id}")]
        public async Task<ActionResult<JobStatusDTO>> GetJobStatus(Guid job_id)
        {
            var job = await _priceService.GetJobStatus(job_id);
            if (job == null) return NotFound();
            return Ok(job);
        }
    }
}
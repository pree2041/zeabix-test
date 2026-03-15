using Pricing.API.DTOs;
using Pricing.API.Models;

namespace Pricing.API.Service.Contracts
{
    public interface IPriceService
    {
        Task<QuoteResponseDTO> CalculatePrice(QuoteRequestDTO request);
        Task<BulkQuotesResponseDTO> SubmitBulkQuotes(CreateBulkQuotesDTO request);
        Task<JobStatusDTO?> GetJobStatus(Guid jobId);
        Task ProcessBulkJob(Guid jobId);
    }
}
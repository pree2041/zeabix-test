using Pricing.API.Models;

namespace Pricing.API.Service.Contracts
{
    public interface IPriceRepository
    {
        Task<Job> CreateJob(List<QuoteRequest> requests);
        Task<Job?> GetJob(Guid jobId);
        Task UpdateJob(Job job);
    }
}
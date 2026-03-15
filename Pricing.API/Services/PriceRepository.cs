using Pricing.API.Models;
using Pricing.API.Service.Contracts;

namespace Pricing.API.Services
{
    public class PriceRepository : IPriceRepository
    {
        private static readonly List<Job> _jobs = new List<Job>();

        public Task<Job> CreateJob(List<QuoteRequest> requests)
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Requests = requests
            };
            _jobs.Add(job);
            return Task.FromResult(job);
        }

        public Task<Job?> GetJob(Guid jobId)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == jobId);
            return Task.FromResult(job);
        }

        public Task UpdateJob(Job job)
        {
            var existing = _jobs.FirstOrDefault(j => j.Id == job.Id);
            if (existing != null)
            {
                existing.Status = job.Status;
                existing.Results = job.Results;
                existing.Error = job.Error;
            }
            return Task.CompletedTask;
        }
    }
}
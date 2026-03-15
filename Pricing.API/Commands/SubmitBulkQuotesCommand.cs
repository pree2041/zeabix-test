using Pricing.API.Interfaces;
using Pricing.API.Models;

namespace Pricing.API.Commands
{
    public class SubmitBulkQuotesCommand : IRequest
    {
        public Guid JobId { get; set; }
        public List<QuoteRequest> Requests { get; set; } = new List<QuoteRequest>();
    }
}
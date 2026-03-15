using Pricing.API.Commands;
using Pricing.API.Interfaces;
using Pricing.API.Service.Contracts;

namespace Pricing.API.Handlers
{
    public class SubmitBulkQuotesCommandHandler : IRequestHandler<SubmitBulkQuotesCommand>
    {
        private readonly IPriceService _priceService;

        public SubmitBulkQuotesCommandHandler(IPriceService priceService)
        {
            _priceService = priceService;
        }

        public async Task HandleAsync(SubmitBulkQuotesCommand request)
        {
            await _priceService.ProcessBulkJob(request.JobId);
        }
    }
}
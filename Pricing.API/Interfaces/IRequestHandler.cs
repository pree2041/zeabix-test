namespace Pricing.API.Interfaces
{
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        Task HandleAsync(TRequest request);
    }
}

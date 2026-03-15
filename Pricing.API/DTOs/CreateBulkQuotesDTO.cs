namespace Pricing.API.DTOs
{
    public class CreateBulkQuotesDTO
    {
        public List<QuoteRequestDTO> Requests { get; set; } = new List<QuoteRequestDTO>();
    }
}
namespace Pricing.API.DTOs
{
    public class BulkQuotesResponseDTO
    {
        public Guid JobId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
    }
}
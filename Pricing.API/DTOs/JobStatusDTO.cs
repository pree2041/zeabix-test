namespace Pricing.API.DTOs
{
    public class JobStatusDTO
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public List<QuoteResponseDTO>? Results { get; set; }
        public string? Error { get; set; }
    }
}
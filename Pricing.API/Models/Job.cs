namespace Pricing.API.Models
{
    public class Job
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public DateTime CreatedAt { get; set; }
        public List<QuoteRequest> Requests { get; set; } = new List<QuoteRequest>();
        public List<QuoteResponse> Results { get; set; } = new List<QuoteResponse>();
        public string? Error { get; set; }
    }
}
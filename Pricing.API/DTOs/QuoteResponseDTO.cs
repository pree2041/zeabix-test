namespace Pricing.API.DTOs
{
    public class QuoteResponseDTO
    {
        public double FinalPrice { get; set; }
        public List<string> AppliedRules { get; set; } = new List<string>();
    }
}
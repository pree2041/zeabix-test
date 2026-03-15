namespace Pricing.API.Models
{
    public class QuoteResponse
    {
        public double FinalPrice { get; set; }
        public List<string> AppliedRules { get; set; } = new List<string>();
    }
}
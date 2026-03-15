namespace Pricing.API.Models
{
    public class TimeWindowConfig
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public double Discount { get; set; } // e.g. 0.1 for 10% discount
    }
}
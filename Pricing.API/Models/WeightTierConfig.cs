namespace Pricing.API.Models
{
    public class WeightTierConfig
    {
        public List<WeightTier> Tiers { get; set; } = new List<WeightTier>();
    }

    public class WeightTier
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Rate { get; set; }
    }
}
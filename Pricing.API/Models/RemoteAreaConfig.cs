namespace Pricing.API.Models
{
    public class RemoteAreaConfig
    {
        public List<string> Areas { get; set; } = new List<string>();
        public double Surcharge { get; set; }
    }
}
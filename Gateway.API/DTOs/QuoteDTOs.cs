using System;

namespace Gateway.API.DTOs
{
    public class QuoteRequestDTO
    {
        public double Weight { get; set; }
        public string Area { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public double BasePrice { get; set; }
    }
}
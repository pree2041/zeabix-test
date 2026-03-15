using AutoMapper;
using Pricing.API.DTOs;
using Pricing.API.Models;

namespace Pricing.API.Mappings
{
    public class PriceProfile : Profile
    {
        public PriceProfile()
        {
            CreateMap<QuoteRequestDTO, QuoteRequest>();
            CreateMap<QuoteResponse, QuoteResponseDTO>();
        }
    }
}
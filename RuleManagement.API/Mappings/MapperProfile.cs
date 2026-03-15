using AutoMapper;
using RuleManagement.API.DTOs;
using RuleManagement.API.Models;

namespace RuleManagement.API.Mappings
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Rule, RuleDTO>();
            CreateMap<CreateRuleDTO, Rule>();
            CreateMap<UpdateRuleDTO, Rule>();
        }
    }
}
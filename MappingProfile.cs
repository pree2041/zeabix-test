using AutoMapper;
using RuleManagement.API.DTOs;
using RuleManagement.API.Models;

namespace RuleManagement.API.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateRuleDTO, Rule>();
            CreateMap<UpdateRuleDTO, Rule>();
            CreateMap<Rule, RuleDTO>();
        }
    }
}
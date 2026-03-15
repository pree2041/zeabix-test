using Pricing.API.Models;
using Pricing.API.Service.Contracts;
using Pricing.API.Services;
using Pricing.API.Infrastructure;
using Moq;
using Xunit;
using Pricing.API.DTOs;
using AutoMapper;


namespace Pricing.API.Tests;

public class PriceServiceTests
{
    private readonly IPriceRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly IMapper _mapper;

    public PriceServiceTests()
    {
        _repository = new PriceRepository();
        _httpClient = new HttpClient();
        _mockMessageBus = new Mock<IMessageBus>();
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<QuoteRequestDTO, QuoteRequest>();
            cfg.CreateMap<QuoteRequest, QuoteRequestDTO>();
        }));
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["RuleServiceUrl"] = "http://localhost:5209"
        }).Build();
        _configuration = config;
    }

    [Fact]
    public async Task CalculatePrice_WithNoRules_ReturnsBasePrice()
    {

        var service = new PriceService(_repository, _httpClient, _configuration, _mockMessageBus.Object, _mapper);
        var request = new QuoteRequestDTO { Weight = 10, Area = "local", Time = DateTime.Now, BasePrice = 100 };

        var response = await service.CalculatePrice(request);

        Assert.Equal(100, response.FinalPrice);
        Assert.Empty(response.AppliedRules);
    }

}

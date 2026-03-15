using System.Text.Json;
using Pricing.API.Models;
using Pricing.API.Service.Contracts;
using Pricing.API.Infrastructure;
using Pricing.API.Commands;
using Pricing.API.DTOs;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using AutoMapper;

namespace Pricing.API.Services
{
    public class PriceService : IPriceService
    {
        private readonly IPriceRepository _repository;
        private readonly HttpClient _httpClient;
        private readonly IMessageBus _messageBus;
        private readonly IMapper _mapper;
        public PriceService(IPriceRepository repository, HttpClient httpClient, IConfiguration configuration, IMessageBus messageBus,IMapper mapper)
        {
            _repository = repository;
            _httpClient = httpClient;
            _messageBus = messageBus;
            _httpClient.BaseAddress = new Uri(configuration["RuleServiceUrl"] ?? "http://localhost:5001");
            _mapper = mapper;
        }

        public async Task<QuoteResponseDTO> CalculatePrice(QuoteRequestDTO request)
        {
            var quoteRequest = _mapper.Map<QuoteRequest>(request);
            var rules = await GetActiveRules();
            var result = CalculatePriceInternal(quoteRequest, rules);

            return new QuoteResponseDTO
            {
                FinalPrice = result.FinalPrice,
                AppliedRules = result.AppliedRules
            };
        }

        private QuoteResponse CalculatePriceInternal(QuoteRequest request, List<Rule> rules)
        {
            double price = request.BasePrice;
            var appliedRules = new List<string>();

            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                if (ApplyRule(rule, request, ref price))
                    appliedRules.Add(rule.RuleName);
            }
            return new QuoteResponse { FinalPrice = price, AppliedRules = appliedRules };
        }

        public async Task<BulkQuotesResponseDTO> SubmitBulkQuotes(CreateBulkQuotesDTO request)
        {

            var quoteRequests = _mapper.Map<List<QuoteRequest>>(request.Requests);
            var job = await _repository.CreateJob(quoteRequests);

            var command = new SubmitBulkQuotesCommand
            {
                JobId = job.Id,
                Requests = quoteRequests
            };

            await _messageBus.PublishAsync(command);

            return new BulkQuotesResponseDTO
            {
                JobId = job.Id,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                TotalRequests = job.Requests.Count,
                CompletedRequests = 0
            };
        }

        public async Task<JobStatusDTO?> GetJobStatus(Guid jobId)
        {
            var job = await _repository.GetJob(jobId);
            if (job == null) return null;

            return new JobStatusDTO
            {
                Id = job.Id,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                TotalRequests = job.Requests.Count,
                CompletedRequests = job.Results?.Count ?? 0,
                Results = job.Results?.Select(r => new QuoteResponseDTO
                {
                    FinalPrice = r.FinalPrice,
                    AppliedRules = r.AppliedRules
                }).ToList(),
                Error = job.Error
            };
        }

        private async Task<List<Rule>> GetActiveRules()
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/rules");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var rules = JsonSerializer.Deserialize<List<Rule>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return rules?.Where(r => r.IsActive && IsEffective(r)).ToList() ?? new List<Rule>();
            }
            catch
            {
                throw new Exception("Internal server error");
            }

        }

        private bool IsEffective(Rule rule)
        {
            var now = DateTime.UtcNow;
            return rule.EffectiveFrom <= now && (!rule.EffectiveTo.HasValue || rule.EffectiveTo >= now);
        }

        private bool ApplyRule(Rule rule, QuoteRequest request, ref double price)
        {
            switch (rule.RuleType)
            {
                case "WeightTier":
                    return ApplyWeightTier(rule, request, ref price);
                case "RemoteAreaSurcharge":
                    return ApplyRemoteAreaSurcharge(rule, request, ref price);
                case "TimeWindowPromotion":
                    return ApplyTimeWindowPromotion(rule, request, ref price);
                default:
                    return false;
            }
        }

        private bool ApplyWeightTier(Rule rule, QuoteRequest request, ref double price)
        {
            var config = JsonSerializer.Deserialize<WeightTierConfig>(rule.ConfigJson);
            if (config == null) return false;
            var tier = config.Tiers.FirstOrDefault(t => request.Weight >= t.Min && request.Weight <= t.Max);
            if (tier != null)
            {
                price *= tier.Rate;
                return true;
            }
            return false;
        }

        private bool ApplyRemoteAreaSurcharge(Rule rule, QuoteRequest request, ref double price)
        {
            var config = JsonSerializer.Deserialize<RemoteAreaConfig>(rule.ConfigJson);
            if (config == null) return false;
            if (config.Areas.Contains(request.Area, StringComparer.OrdinalIgnoreCase))
            {
                price += config.Surcharge;
                return true;
            }
            return false;
        }

        private bool ApplyTimeWindowPromotion(Rule rule, QuoteRequest request, ref double price)
        {
            var config = JsonSerializer.Deserialize<TimeWindowConfig>(rule.ConfigJson);
            if (config == null) return false;
            var time = request.Time.TimeOfDay;
            if (time >= config.Start && time <= config.End)
            {
                price *= (1 - config.Discount);
                return true;
            }
            return false;
        }

        public async Task ProcessBulkJob(Guid jobId)
        {
            var job = await _repository.GetJob(jobId);
            if (job == null) return;

            job.Status = "Processing";
            await _repository.UpdateJob(job);

            try
            {
                var rules = await GetActiveRules();
                var results = new List<QuoteResponse>();
                foreach (var request in job.Requests)
                {
                    var result = CalculatePriceInternal(request, rules);
                    results.Add(result);
                }
                job.Results = results;
                job.Status = "Completed";
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.Error = ex.Message;
            }

            await _repository.UpdateJob(job);
        }
    }
}
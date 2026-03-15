using System.Text;
using System.Net;
using Gateway.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Gateway.API.Tests
{
    public class QuotesControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public QuotesControllerTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["PricingServiceUrl"]).Returns("http://localhost:5200");

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        private IFormFile CreateMockFile(string content, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName);
        }

        [Fact]
        public async Task SubmitBulk_WithValidCsv_ReturnsOk()
        {
            // Arrange
            var csv = "Weight,Area,Time,BasePrice\n1.5,BKK,2024-03-14T12:00:00,100";
            var file = CreateMockFile(csv, "quotes.csv");

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"jobId\":\"123\"}") });

            var controller = new QuotesController(_httpClient, _mockConfig.Object);

            // Act
            var result = await controller.SubmitBulk(file);

            // Assert
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
        }

        [Fact]
        public async Task SubmitBulk_WithInvalidCsvColumns_ReturnsBadRequest()
        {
            // Arrange
            var csv = "Weight,Area\n1.5,BKK"; // Missing columns
            var file = CreateMockFile(csv, "invalid.csv");
            var controller = new QuotesController(_httpClient, _mockConfig.Object);

            // Act
            var result = await controller.SubmitBulk(file);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid or unsupported file format.", badRequest.Value);
        }

        [Fact]
        public async Task SubmitBulk_WithValidJson_ReturnsOk()
        {
            // Arrange
            var json = "[{\"Weight\": 2.0, \"Area\": \"HK\", \"Time\": \"2024-03-14T10:00:00\", \"BasePrice\": 50}]";
            var file = CreateMockFile(json, "quotes.json");

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

            var controller = new QuotesController(_httpClient, _mockConfig.Object);

            // Act
            var result = await controller.SubmitBulk(file);

            // Assert
            Assert.IsType<ContentResult>(result);
        }

        [Fact]
        public async Task SubmitBulk_WithEmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var file = CreateMockFile("", "empty.csv");
            var controller = new QuotesController(_httpClient, _mockConfig.Object);

            // Act
            var result = await controller.SubmitBulk(file);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
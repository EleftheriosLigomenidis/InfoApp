using Azure;
using FakeItEasy;
using FluentAssertions;
using IPInfoApp.Business.Exceptions;
using IPInfoApp.Business.Services;
using IPInfoApp.Business.Utils;
using IPInfoApp.Data.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Tests
{
    public class Ip2cServiceTests
    {
        private readonly Ip2cService _ip2cService;
        private readonly HttpClient _httpClient;
        private readonly FakeableHttpMessageHandler _httpMessageHandler;
        private readonly ILogger<Ip2cService> _logger;

        public Ip2cServiceTests()
        {
            _httpMessageHandler = A.Fake<FakeableHttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://ip2c.org/") 
            };

            _logger = A.Fake<ILogger<Ip2cService>>();
            _ip2cService = new Ip2cService(_httpClient, _logger);
        }

        [Fact]
        public async Task GetCountryDetailsByIpAsync_ShouldReturnCountry_WhenResponseIsSuccessful()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            var responseContent = "1;US;USA;United States";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            A.CallTo(() => _httpMessageHandler.FakeSendAsync(
                   A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored))
               .Returns(responseMessage);


            // Act
            var result = await _ip2cService.GetCountryDetailsByIpAsync(ipAddress);

            // Assert
            result.Should().NotBeNull();
            result.TwoLetterCode.Should().Be("US");
            result.ThreeLetterCode.Should().Be("USA");
            result.Name.Should().Be("United States");
        }

        [Fact]
        public async Task GetCountryDetailsByIpAsync_ShouldThrowIP2CException_WhenResponseIsUnsuccessful()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request")
            };

            // Setup the fake to return a BadRequest response when SendAsync is called
            A.CallTo(() => _httpMessageHandler.FakeSendAsync(A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(responseMessage));

            // Act
            Func<Task> act = async () => await _ip2cService.GetCountryDetailsByIpAsync(ipAddress);

            // Assert
            await act.Should().ThrowAsync<IP2CException>()
                .WithMessage("IP2C Web Service Error: BadRequest - Bad Request");
        }

        [Fact]
        public async Task GetCountryDetailsByIpAsync_ShouldThrowIP2CException_WhenIpAddressIsInvalid()
        {
            // Arrange
            var ipAddress = "8";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request")
            };

            // Setup the fake to return a BadRequest response when SendAsync is called
            A.CallTo(() => _httpMessageHandler.FakeSendAsync(A<HttpRequestMessage>.Ignored, A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(responseMessage));

            // Act
            Func<Task> act = async () => await _ip2cService.GetCountryDetailsByIpAsync(ipAddress);

            // Assert
            await act.Should().ThrowAsync<IP2CException>()
                .WithMessage("IP2C Web Service Error: BadRequest - Bad Request");
        }

        [Fact]
        public async Task GetCountryDetailsByIpsAsync_ShouldReturnDictionaryOfCountries_WhenAllResponsesAreSuccessful()
        {
            // Arrange
            var ipAddresses = new List<string> { "8.8.8.8", "8.8.4.4" };

            // Setup successful responses for each IP address
            var responseContent1 = "1;US;USA;United States";
            var responseContent2 = "1;GB;GBR;United Kingdom";
            var responseMessage1 = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent1)
            };
            var responseMessage2 = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent2)
            };

            A.CallTo(() => _httpMessageHandler.FakeSendAsync(A<HttpRequestMessage>.That.Matches(r => r.RequestUri.AbsoluteUri.Contains("8.8.8.8")), A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(responseMessage1));
            A.CallTo(() => _httpMessageHandler.FakeSendAsync(A<HttpRequestMessage>.That.Matches(r => r.RequestUri.AbsoluteUri.Contains("8.8.4.4")), A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(responseMessage2));

            // Act
            var result = await _ip2cService.GetCountryDetailsByIpsAsync(ipAddresses);

            // Assert
            result.Should().HaveCount(2);
            result["8.8.8.8"].Name.Should().Be("United States");
            result["8.8.4.4"].Name.Should().Be("United Kingdom");
        }

        [Fact]
        public async Task GetCountryDetailsByIpsAsync_ShouldReturnOneSuccesfulResponse_WhenGivenTwoIpAddressesAndOfThemIsCorrect()
        {
            // Arrange
            var ipAddresses = new List<string> { "8.8.8.8", "8.8.4.4" };

            var responseContent1 = "1;US;USA;United States";
            var responseMessage1 = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent1)
            };

            var responseMessage2 = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request")
            };


            A.CallTo(() => _httpMessageHandler.FakeSendAsync(A<HttpRequestMessage>.That.Matches(r => r.RequestUri.AbsoluteUri.Contains("8.8.8.8")), A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(responseMessage1));
            A.CallTo(() => _httpMessageHandler.FakeSendAsync(A<HttpRequestMessage>.That.Matches(r => r.RequestUri.AbsoluteUri.Contains("8.8.4.4")), A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(responseMessage2));

            // Act
            var result = await _ip2cService.GetCountryDetailsByIpsAsync(ipAddresses);

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainKey("8.8.8.8");
            result["8.8.8.8"].Name.Should().Be("United States");
            result.Should().NotContainKey("8.8.4.4");

        }
    }
}

using FakeItEasy;
using FluentAssertions;
using IPInfoApp.API.Controllers;
using IPInfoApp.Business.Contracts;
using IPInfoApp.Business.Models;
using IPInfoApp.Business.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.API.Tests
{
    public class IpInformationControllerUnitTests
    {
        private readonly IpInformationController _sut;
        private readonly IIpInformationService _service;
        public IpInformationControllerUnitTests()
        {
            _service = A.Fake<IIpInformationService>();
            _sut = new IpInformationController(_service);
        }

      
        [Fact]
        public async Task GetIpInformationAsync_ShouldThrowArgumentNullException_WhenIpAddressIsNull()
        {
            // Arrange
            string ipAddress = null;

            // Act
            Func<Task> act = async () => await _sut.GetIpInformationAsync(ipAddress);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*ipAddress*");
        }


        [Fact]
        public async Task GetIpInformationAsync_ShouldReturnOkObjectResult_WithCountryDetails_WhenIpAddressIsValid()
        {
            // Arrange
            string ipAddress = "127.0.0.1";
            var countryDetails = new Country { Id =1 , ThreeLetterCode = "Test", TwoLetterCode = "Test",Name ="Test" };
            A.CallTo(() => _service.GetIpInformationAsync(ipAddress)).Returns(Task.FromResult(countryDetails));

            // Act
            var result = await _sut.GetIpInformationAsync(ipAddress);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(countryDetails);
        }
        [Fact]
        public async Task GetIpAddressReport_ShouldReturnOkObjectResult_WithEmptyList_WhenCountryCodesAreNull()
        {
            // Arrange
            List<string>? countryCodes = null;
            var expectedResults = new List<IpAddressPerCountry>();
            A.CallTo(() => _service.GetIpAddressReportAsync(countryCodes)).Returns(Task.FromResult(expectedResults));

            // Act
            var result = await _sut.GetIpAddressReport(countryCodes);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(expectedResults);
        }

        [Fact]
        public async Task GetIpAddressReport_ShouldReturnOkObjectResult_WithEmptyList_WhenCountryCodesAreEmpty()
        {
            // Arrange
            var countryCodes = new List<string>();
            var expectedResults = new List<IpAddressPerCountry>();
            A.CallTo(() => _service.GetIpAddressReportAsync(countryCodes)).Returns(Task.FromResult(expectedResults));

            // Act
            var result = await _sut.GetIpAddressReport(countryCodes);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(expectedResults);
        }
        [Fact]
        public async Task GetIpAddressReport_ShouldReturnOkObjectResult_WithIpReport_WhenProvidedWithCountryCodes()
        {
            // Arrange
            var countryCodes = new List<string> { "US", "CA" };
            var expectedResults = new List<IpAddressPerCountry>
        {
            new IpAddressPerCountry { CountryName = "USA", AddressesCount = 100,LastAddressUpdated = new DateTime(2024,1,1) },
            new IpAddressPerCountry { CountryName = "CANADA", AddressesCount = 50,LastAddressUpdated = new DateTime(2024,1,1) }
        };
            A.CallTo(() => _service.GetIpAddressReportAsync(countryCodes)).Returns(Task.FromResult(expectedResults));

            // Act
            var result = await _sut.GetIpAddressReport(countryCodes);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(expectedResults);
        }
    }
}

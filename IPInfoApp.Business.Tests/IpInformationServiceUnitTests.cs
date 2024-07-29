using FakeItEasy;
using FluentAssertions;
using IPInfoApp.Business.Contracts;
using IPInfoApp.Business.Exceptions;
using IPInfoApp.Business.Mapper;
using IPInfoApp.Business.Services;
using IPInfoApp.Data.IRepositories;
using IPInfoApp.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Tests
{
    public class IpInformationServiceUnitTests : IDisposable, IAsyncDisposable
    {
    
        private readonly ApplicationDbContext _context;
        private readonly SqliteConnection _connection;
        private readonly IQueryable<Country> queryable;
        private readonly IpInformationService _sut;
        private readonly ILogger<IpInformationService> _logger;
        private readonly ICacheService _redis;
        private readonly IIp2cService _ip2c;
        private readonly IReportRepository _reportRepository;
        public IpInformationServiceUnitTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            //SeedProducts();
            queryable = _context.Countries.AsQueryable();
            _logger = A.Fake<ILogger<IpInformationService>>();
            _redis = A.Fake<ICacheService>();
            _reportRepository = A.Fake<IReportRepository>();
            _ip2c = A.Fake<IIp2cService>();
            _sut = new IpInformationService(_context, _logger, _redis, _ip2c, _reportRepository);
        }



        [Fact]
        public async Task GetIpInformationAsync_ShouldReturnCountryFromCache_WhenThereIsAlrealdyOneStored()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            var cachedCountry = new Models.Country { Id = 1, Name = "CachedCountry" };
            A.CallTo(() => _redis.TryGetValueFromCacheAsync<Models.Country>(ipAddress))
                .Returns(cachedCountry);

            // Act
            var result = await _sut.GetIpInformationAsync(ipAddress);

            // Assert
            result.Should().BeEquivalentTo(cachedCountry);
        }

        [Fact]
        public async Task GetIpInformationAsync_ShouldFetchCountry_FromExternalService_WhenNotInCacheOrDb()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            var externalCountry = new Models.Country { Id = 3, Name = "ExternalCountry" };
            A.CallTo(() => _redis.TryGetValueFromCacheAsync<Models.Country>(ipAddress))
                .Returns(Task.FromResult<Models.Country>(null));
    
            A.CallTo(() => _ip2c.GetCountryDetailsByIpAsync(ipAddress))
                .Returns(Task.FromResult(externalCountry));

            // Act
            var result = await _sut.GetIpInformationAsync(ipAddress);

            // Assert
            result.Should().BeEquivalentTo(externalCountry);
        }

        [Fact]
        public async Task GetIpInformationAsync_ShouldThrowException_WhenCountryNotFound()
        {
            // Arrange
            var ipAddress = "192.168.1.4";
            A.CallTo(() => _redis.TryGetValueFromCacheAsync<Models.Country>(ipAddress))
                .Returns(Task.FromResult<Models.Country>(null));
            A.CallTo(() => _ip2c.GetCountryDetailsByIpAsync(ipAddress))
                .Returns(Task.FromResult<Models.Country>(null));

            // Act
            Func<Task> act = async () => await _sut.GetIpInformationAsync(ipAddress);

            // Assert
            await act.Should().ThrowAsync<CountryNotFoundException>();
        }

        private async Task SeedDbAsync()
        {
          var dummyCountry =  new Data.Models.Country {
                Id = 2,
                Name = "DbCountry",
                CreatedAt = new DateTime(2024, 4, 4),
                TwoLetterCode = "Test",
                ThreeLetterCode = "test" ,
                IpAddresses = new List<IpAddress>()
                {
                    new IpAddress()
                    {
                        CreatedAt =new DateTime(2024,4,4),
                        UpdatedAt =new DateTime(2024,4,4),
                        CountryId = 2,
                        Id = 2,
                        Ip = "192.168.1.2"
                    }
                }
            };

            _context.Countries.Add(dummyCountry);
           await _context.SaveChangesAsync();
        }


        [Fact]
        public async Task GetIpInformationAsync_ShouldReturnCountry_FromDatabase_WhenNotInCacheAndAlreadyExistsInDb()
        {
            // Arrange
            var ipAddress = "192.168.1.2";
            var srvCountry = new Models.Country
            {
                Id = 2,
                Name = "DbCountry",
                CreatedAt = new DateTime(2024, 4, 4),
                TwoLetterCode = "Test",
                ThreeLetterCode = "test",
                IpAddresses = new List<Models.IpAddress>()
                    {
                        new Models.IpAddress()
                        {
                            CreatedAt =new DateTime(2024,4,4),
                            UpdatedAt =new DateTime(2024,4,4),
                            CountryId = 2,
                            Id = 2,
                            Ip = "192.168.1.2"
                        }
                }
            };
            var dbCountry =  srvCountry.ToDbObject();
            A.CallTo(() => _redis.TryGetValueFromCacheAsync<Models.Country?>(ipAddress))
                .Returns(Task.FromResult<Models.Country?>(null));


            // Act
            await SeedDbAsync();
            var result = await _sut.GetIpInformationAsync(ipAddress);

            // Assert
            result.Should().BeEquivalentTo(srvCountry);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}

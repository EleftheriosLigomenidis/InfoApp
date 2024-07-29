using Hangfire;
using IPInfoApp.Business.Contracts;
using IPInfoApp.Business.Exceptions;
using IPInfoApp.Business.Mapper;
using IPInfoApp.Business.Models;
using IPInfoApp.Business.Utils;
using IPInfoApp.Data.Enums;
using IPInfoApp.Data.IRepositories;
using IPInfoApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Pipelines.Sockets.Unofficial.SocketConnection;

namespace IPInfoApp.Business.Services
{
    public class IpInformationService: IIpInformationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IpInformationService> _logger;
        private readonly ICacheService _redis;
        private readonly IIp2cService _ip2c;
        private readonly IReportRepository _reportRepository;

        public IpInformationService(ApplicationDbContext context,
                                    ILogger<IpInformationService> logger,
                                    ICacheService redis,
                                    IIp2cService ip2CService,
                                    IReportRepository reportRepository)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
            _ip2c = ip2CService;
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// Gets the information of a country using its ip address
        /// </summary>
        /// <param name="ipAddress">The ip address</param>
        /// <returns></returns>
        public async Task<Models.Country> GetIpInformationAsync(string ipAddress)
        {
            string key = $"{ipAddress}";
            Models.Country? country; 
            try
            {
                
                 country = await _redis.TryGetValueFromCacheAsync<Models.Country>(key);

                if (country == null)
                {
                    
                    country = await TryGetCountryFromDbAsync(ipAddress);

                    country ??= await _ip2c.GetCountryDetailsByIpAsync(ipAddress);
    
                    if (country == null)
                        throw new CountryNotFoundException(ipAddress);

                    await SaveCountryInformationInDb(country, ipAddress);
                    await _redis.SetCacheItemAsync(key, country);
                }
                return country;
            }
            catch (Exception ex)
            {

                _logger.LogWarning(ex,Messages.FetchInformation(ipAddress));
                throw;
            }
       
        }


        /// <summary>
        /// Fetches the country details from the db by the ip address
        /// </summary>
        /// <param name="ipAddress">The ipAddress</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Models.Country?> TryGetCountryFromDbAsync(string ipAddress)
        {

            _logger.LogInformation(Messages.FetchEntity(nameof(Models.Country), nameof(ipAddress), ipAddress, Datasource.Database.GetEnumDescription()));
            Models.Country? country = null;
            Data.Models.IpAddress? dbObject = await _context.IpAddresses
                                 .Include(x => x.Country)
                                 .FirstOrDefaultAsync(x => x.Ip == ipAddress);

            //if (dbObject is null)
            //{
            //    var errorMessage = Messages.FetchEntity(nameof(Data.Models.IpAddress), nameof(ipAddress), ipAddress);
            //    _logger.LogError(errorMessage);
            //     throw new KeyNotFoundException(errorMessage);
            //}
            if(dbObject != null)
            {

              country =  CountryMapper.ToBusinessObject(dbObject.Country);
            }

            return country;
        }

        /// <summary>
        /// Fetches a list of ip address per country statistics by country code
        /// </summary>
        /// <param name="countryCodes">The country codes</param>
        /// <returns></returns>
        public async Task<List<Models.IpAddressPerCountry>> GetIpAddressReportAsync(List<string>? countryCodes)
        {
            string countryCodesString = countryCodes is not null &&  countryCodes.Count > 0 ? string.Join(",",countryCodes) : string.Empty;
            string key = $"{countryCodesString}";

             
            List<Models.IpAddressPerCountry>? reportItems;

            reportItems = await _redis.TryGetValueFromCacheAsync<List<Models.IpAddressPerCountry>>(key);
            if (reportItems == null) 
            {
                reportItems = [];
                try
                {
                    var dbItems = await _reportRepository.GetIpAddressesPerCountryAsync(countryCodesString);
                    reportItems = dbItems.Select(ReportMapper.ToBusinessObject).ToList();
                    await _redis.SetCacheItemAsync(key, reportItems);
                }
                catch (Exception ex)
                {
                    var message = Messages.FetchCollectionFailed(nameof(Data.Models.IpAddressPerCountry));
                    _logger.LogWarning(ex, message);
                    throw;
                }
            }
         

            return reportItems;
        }

        /// <summary>
        /// Fetches the ip information from the db and updates them in batches of 100
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)] // Timeout after 5 minutes
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task UpdateIpInformation(CancellationToken cancellationToken)
        {
             _logger.LogInformation(Messages.UpdateIpInformationStarted());
            const int batchSize = 100;
            var totalCount = await _context.IpAddresses.CountAsync(cancellationToken: cancellationToken);
            var totalBatches = (int)Math.Ceiling((double)totalCount / batchSize);

            //Fetch all the countries from start   solves the N + 1 problem
            // Downside is if dataset is large
            var countries = await _context.Countries.ToListAsync(cancellationToken);
            Dictionary<string,Data.Models.Country> countryLookup =  countries.ToDictionary(x => x.TwoLetterCode,x => x);
            Dictionary<int, string> twoLetterCodeLookup = countries.ToDictionary(x => x.Id, x => x.TwoLetterCode);
            Dictionary<string, int> idLookup = countries.ToDictionary(x => x.TwoLetterCode, x => x.Id);
            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    for (int batchNumber = 0; batchNumber < totalBatches; batchNumber++)
                    {
                        

                        var ipAddresses = await _context.IpAddresses
                                            .Skip(batchNumber * batchSize)
                                            .Take(batchSize)
                                            .ToListAsync(cancellationToken: cancellationToken);

                        //get updated information for ipAddress form webservice ip2c
                        List<string> ips = ipAddresses.Select(x => x.Ip).ToList();
                        
                        Dictionary<string, Models.Country> ipCountryWebDictionary = await _ip2c.GetCountryDetailsByIpsAsync(ips);

                        List<Data.Models.Country> newCountries = await SaveNewCountries(ipCountryWebDictionary, countryLookup);

                        if (newCountries.Count != 0)
                        {
                            foreach (var country in newCountries)
                            {
                                idLookup[country.TwoLetterCode] = country.Id;
                                twoLetterCodeLookup[country.Id] = country.TwoLetterCode;
                            }
                        }
                      

                        await  UpdateIpAddresses(twoLetterCodeLookup, idLookup, ipCountryWebDictionary, ipAddresses);
                    }
          
                    await transaction.CommitAsync(cancellationToken);
                }
                catch(Exception ex)
                {
                    //  any /all changes from prior iterations will be rolledbacked
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex,Messages.UpdateIpInformationFailed());
                    throw;   
                }
            }
                
        }

        private async Task<List<Data.Models.Country>> SaveNewCountries(Dictionary<string, Models.Country> ipCountryWebDictionary, 
                                                                       Dictionary<string,Data.Models.Country> dbCountryLookup)
        {
            var countryCodeList = ipCountryWebDictionary.Values.DistinctBy(x => x.TwoLetterCode).ToList();
            List<Data.Models.Country> newCountries = new();
            foreach (var country in countryCodeList) 
            {
                if(!dbCountryLookup.ContainsKey(country.TwoLetterCode))
                { 
                    var dbCountry = CountryMapper.ToDbObject(country);
                    newCountries.Add(dbCountry);
                    
                }
            }

             await  _context.Countries.BulkInsertAsync(newCountries);
            return newCountries;
        }

        /// <summary>
        /// Updates the ip addresses
        /// </summary>
        /// <param name="twoLetterCodeLookup">A dictionary that contains  id/two letter code lookup</param>
        /// <param name="idLookup">A dicitonary that contains  country code / id  lookup</param>
        /// <param name="ipCountryWebDictionary">Dictionary that contains  ip key / country lookup</param>
        /// <param name="ipAddresses">The db ip addresses</param>
        /// <returns></returns>
        private async Task UpdateIpAddresses(Dictionary<int, string> twoLetterCodeLookup,
                                              Dictionary<string, int> idLookup,
                                                 Dictionary<string, Models.Country> ipCountryWebDictionary,
                                                 List<Data.Models.IpAddress> ipAddresses)
            {
            var cacheKeysToUpdate = new List<string>();
            var updatedCountriesDictionary = new Dictionary<string, Models.Country>(); 
            foreach (var ipAddress in ipAddresses) 
                {
                    // for the specific ip search if there is country information
                    if (ipCountryWebDictionary.TryGetValue(ipAddress.Ip, out Models.Country? countryFromWebService))
                    {
                        var countryId = ipAddress.CountryId;
                        //check if the ip information matches the country in the dp
                        if (twoLetterCodeLookup[countryId] != countryFromWebService.TwoLetterCode)
                        {
                            
                           ipAddress.CountryId = idLookup[countryFromWebService.TwoLetterCode];

                        updatedCountriesDictionary[ipAddress.Ip] = countryFromWebService;
                        }
                     
                    }
                }

               await _context.IpAddresses.BulkUpdateAsync(ipAddresses);
               var cacheTasks = cacheKeysToUpdate.Select(async key =>
            {
                await _redis.ClearCacheAsync(key);
                await _redis.SetCacheItemAsync(key, updatedCountriesDictionary[key]);
            });
        }





        /// <summary>
        /// Persists the country details in the db
        /// </summary>
        /// <param name="country">The country details</param>
        /// <param name="ipAddress">The ip address</param>
        /// <returns></returns>
        private async Task SaveCountryInformationInDb(Models.Country country,string ipAddress)
        {
            _logger.LogInformation(Messages.CreatingEntity(nameof(Models.Country)));
            var dbCountry =  await _context.Countries.FirstOrDefaultAsync(x => x.TwoLetterCode == country.TwoLetterCode);
            Models.IpAddress model = new(ipAddress);

            //case country already  exists just add the id to the new ipAddress otherwise create a new coutry 
            Data.Models.IpAddress dbObject = dbCountry  == null ? 
                                             dbObject = model.ToDbObject(country) :
                                             dbObject = model.ToDbObject(dbCountry.Id);

            _context.IpAddresses.Add(dbObject);

            try
            {
               await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.CreateEntityFailed(nameof(Models.Country)));
                throw;
            }
        }
    }
}

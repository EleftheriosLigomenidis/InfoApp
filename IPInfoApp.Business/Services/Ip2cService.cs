using IPInfoApp.Business.Contracts;
using IPInfoApp.Business.Exceptions;
using IPInfoApp.Business.Models;
using IPInfoApp.Business.Utils;
using IPInfoApp.Data.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Services
{
    public class Ip2cService : IIp2cService
    {
        private readonly ILogger<Ip2cService> _logger;
        private readonly HttpClient _httpClient;
        public Ip2cService(HttpClient httpClient, ILogger<Ip2cService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Fetces the information about the country using the IP2C service 
        /// </summary>
        /// <param name="ipAddress">The ip Address with which the country details will be fetched</param>
        /// <returns></returns>
        /// <exception cref="IP2CException">Throws an exception if the call is unsuccesful</exception>
        public async Task<Country?> GetCountryDetailsByIpAsync(string ipAddress)
        {
            _logger.LogInformation(Messages.FetchEntity(nameof(Country),
                                                        nameof(ipAddress),
                                                        ipAddress,
                                                        Datasource.WebService.GetEnumDescription()));

            var response = await _httpClient.GetAsync(ipAddress);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return ProcessSuccessIp2cResponce(responseBody);

            else
                throw new IP2CException(statusCode: response.StatusCode, reasonPhrase: response.ReasonPhrase ?? "", responseBody: responseBody);
        }

        /// <summary>
        /// Processes the response of the ip2c service  and converts it into  a Business object
        /// </summary>
        /// <param name="responseBody">The responce body of the ip2c</param>
        /// <returns></returns>
        private static Country? ProcessSuccessIp2cResponce(string responseBody) 
        {
            Country? country = null;
            if (!string.IsNullOrEmpty(responseBody))
            {
                var data = responseBody.Split(';');
                country = new();
                country.TwoLetterCode = data[1];
                country.ThreeLetterCode = data[2];
                country.Name = data[3];
            }
                return country;
        }

        /// <summary>
        /// Gets all the country details for a list of ipAddreses
        /// </summary>
        /// <param name="ipAddresses">The list of ip addresses</param>
        /// <returns></returns>
        public async Task<Dictionary<string, Country>> GetCountryDetailsByIpsAsync(List<string> ipAddresses)
        {
            _logger.LogInformation(Messages.WebServiceBulkOperation());
            //Dictionary is choses cause its the quickest in memory dataset
            Dictionary<string,Country> keyValues = new();
            //We use when all to await all tasks to complete without blocking 
            var task = ipAddresses.Select(async ip =>
            {
                try
                {
                    var country = await GetCountryDetailsByIpAsync(ip);
                    return(ip,country);

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, Messages.FetchEntityFailed(nameof(Models.Country),Datasource.WebService.GetEnumDescription(),ip));
                    return (ip, null);
                    //Alternate implementation if one task fails the whole operation fails...
                    //Stricter error handling
                    // if information about an ip is not found for any reason 
                    // the process is going to run again in one hour
                    //throw;

                }
            });

            try
            {
                //awaits all tasks and upon completion creates a new task when all tasks are completed
               var results = await Task.WhenAll(task);
                keyValues = results.Where(x => x.country != null).ToDictionary(r => r.ip, r => r.country!);
               return keyValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.WebServiceBulkOperationFailed());
                throw;
            }
        }

    }
}

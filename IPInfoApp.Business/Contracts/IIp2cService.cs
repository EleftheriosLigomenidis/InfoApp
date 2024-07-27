using IPInfoApp.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Contracts
{
    public interface IIp2cService
    {
        Task<Country?> GetCountryDetailsByIpAsync(string ipAddress);
        Task<Dictionary<string, Country>> GetCountryDetailsByIpsAsync(List<string> ipAddresses);
    }
}

using IPInfoApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Data.IRepositories
{
    public interface IReportRepository
    {
        Task<List<IpAddressPerCountry>> GetIpAddressesPerCountryAsync(string? countryCodeList);
    }
}

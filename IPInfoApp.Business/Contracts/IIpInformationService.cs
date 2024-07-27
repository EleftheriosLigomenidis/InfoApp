using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Contracts
{
    public interface IIpInformationService
    {
        Task<Models.Country> GetIpInformationAsync(string ipAddress);
        Task<List<Models.IpAddressPerCountry>> GetIpAddressReportAsync(List<string>? countryCodes);
        Task UpdateIpInformation(CancellationToken cancellationToken);
    }
}

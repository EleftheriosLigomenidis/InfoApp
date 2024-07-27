using IPInfoApp.Business.Contracts;
using IPInfoApp.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IPInfoApp.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IpInformationController(IIpInformationService ipInformationService) : ControllerBase
    {
        private readonly IIpInformationService _ipInformationService = ipInformationService;


        [HttpGet("{ipAddress}")]
        public async Task<IActionResult> GetIpInformationAsync(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))

            {
                throw new ArgumentNullException(nameof(ipAddress));
            }
            else 
            {
                Country coutnryDetails =   await _ipInformationService.GetIpInformationAsync(ipAddress);
                return Ok(coutnryDetails);
            }
                
        }

        [HttpGet("Report")]
        public async Task<IActionResult> GetIpAddressReport([FromQuery] List<string>? countryCodes)
        {
            //Normally using post for simply getting items from db is a violation of the principles of rest
            //However it could be justified here since we can  use the request  body to get the country codes
            //instead of having a large query params and url;
            // avoiding complex quieries or url string limitations
            List<IpAddressPerCountry> results = await  _ipInformationService.GetIpAddressReportAsync(countryCodes);
            return Ok(results);
        }



    }
}

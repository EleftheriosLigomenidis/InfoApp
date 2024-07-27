using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Models
{
    public record IpAddressPerCountry
    {
        public string CountryName { get; set; } = null!;
        public int AddressesCount { get; set; }
        public DateTime LastAddressUpdated { get; set; }
    }
}

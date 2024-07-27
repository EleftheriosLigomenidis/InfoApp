using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Mapper
{
    public static class ReportMapper
    {
        /// <summary>
        /// Expression used to map db objects to service objects
        /// used because it can be translated into sql a functionality used by  ef core
        /// </summary>
        public static readonly Expression<Func<Data.Models.IpAddressPerCountry, Models.IpAddressPerCountry>> IpAddressPerCountry = (report) => new Models.IpAddressPerCountry
        {
          AddressesCount = report.AddressesCount,
          LastAddressUpdated = report.LastAddressUpdated,
          CountryName = report.CountryName,
        };

        /// <summary>
        /// maps a db obj into  service obj 
        /// </summary>
        /// <param name="dbObject">The db object</param>
        /// <returns></returns>
        public static Models.IpAddressPerCountry ToBusinessObject(Data.Models.IpAddressPerCountry dbObject)
        {
            return new Models.IpAddressPerCountry
            {
              CountryName = dbObject.CountryName,
              LastAddressUpdated = dbObject.LastAddressUpdated,
              AddressesCount= dbObject.AddressesCount,
            };
        }

    }
}

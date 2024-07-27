using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Mapper
{
    public static class CountryMapper
    {
        /// <summary>
        /// Expression used to map db objects to service objects
        /// used because it can be translated into sql a functionality used by  ef core
        /// </summary>
        public static readonly Expression<Func<Data.Models.Country, Models.Country>> CountrySelector = (country) => new Models.Country
        {
            Id = country.Id,
            CreatedAt = country.CreatedAt,
            IpAddresses = country.IpAddresses.Select(x => IpAddressMapper.ToBusinessObject(x)).ToList(),
            ThreeLetterCode = country.ThreeLetterCode,
            TwoLetterCode = country.TwoLetterCode,
            Name = country.Name,
        };



        /// <summary>
        /// maps a service obj to db obj
        /// </summary>
        /// <param name="srvObject">The service object</param>
        /// <returns></returns>
        public static Data.Models.Country ToDbObject(this Models.Country srvObject)
        {
            return new Data.Models.Country
            {
                CreatedAt = srvObject.CreatedAt,
                ThreeLetterCode = srvObject.ThreeLetterCode,
                TwoLetterCode = srvObject.TwoLetterCode,
                Name = srvObject.Name,
                IpAddresses = srvObject.IpAddresses.Select(IpAddressMapper.ToDbObject).ToList(),    

            };
        }

        /// <summary>
        /// maps a db obj into  service obj 
        /// </summary>
        /// <param name="dbObject">The db object</param>
        /// <returns></returns>
        public static Models.Country ToBusinessObject(Data.Models.Country dbObject)
        {
            return new Models.Country
            {
                Id = dbObject.Id,
                CreatedAt = dbObject.CreatedAt,
                TwoLetterCode = dbObject.TwoLetterCode,
                Name = dbObject.Name,
                ThreeLetterCode = dbObject.ThreeLetterCode,
                IpAddresses = dbObject.IpAddresses.Select(IpAddressMapper.ToBusinessObject).ToList(),
            };
        }

        /// <summary>
        /// Updates the target with the info of the source
        /// </summary>
        /// <param name="source">The object to be updated</param>
        /// <param name="target">The object that will be use to update the source</param>
        public static void UpdateWith(this Models.Country source, Data.Models.Country target)
        {
            target.ThreeLetterCode = source.ThreeLetterCode;
            target.TwoLetterCode = source.TwoLetterCode;
            target.Name = source.Name;
        }
    }
}

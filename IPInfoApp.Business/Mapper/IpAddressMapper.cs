using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IPInfoApp.Business.Mapper
{
    public static class IpAddressMapper
    {
        /// <summary>
        /// Expression used to map db objects to service objects
        /// used because it can be translated into sql a functionality used by  ef core
        /// </summary>
        public static readonly Expression<Func<Data.Models.IpAddress, Models.IpAddress>> IPAddressSelector = (ipAddress) => new Models.IpAddress
        {
            Id = ipAddress.Id,
            CreatedAt = ipAddress.CreatedAt,
            UpdatedAt = ipAddress.UpdatedAt,
            CountryId = ipAddress.CountryId,
            Ip = ipAddress.Ip,
        };

        /// <summary>
        /// maps a service obj to db obj
        /// </summary>
        /// <param name="srvObject">The service object</param>
        /// <returns></returns>
        public static Data.Models.IpAddress ToDbObject(this Models.IpAddress srvObject)
        {
            return new Data.Models.IpAddress
            {
                CreatedAt = srvObject.CreatedAt,
                UpdatedAt = srvObject.UpdatedAt,
                Country = CountryMapper.ToDbObject(srvObject.Country),
                Ip = srvObject.Ip,

            };
        }

        /// <summary>
        /// maps a db obj into  service obj 
        /// </summary>
        /// <param name="dbObject">The db object</param>
        /// <returns></returns>
        public static Models.IpAddress ToBusinessObject(Data.Models.IpAddress dbObject)
        {
            return new Models.IpAddress
            {
                Id = dbObject.Id,
                CreatedAt = dbObject.CreatedAt,
                UpdatedAt = dbObject.UpdatedAt,
                CountryId = dbObject.CountryId,
                Ip = dbObject.Ip,
            };
        }

        /// <summary>
        /// Updates the target with the info of the source
        /// </summary>
        /// <param name="source">The object to be updated</param>
        /// <param name="target">The object that will be use to update the source</param>
        public static void UpdateWith(this Models.IpAddress source, Data.Models.IpAddress target)
        {
            target.CountryId = source.CountryId;
            target.Ip = source.Ip;
            target.UpdatedAt = DateTime.UtcNow;
        }
    }
}

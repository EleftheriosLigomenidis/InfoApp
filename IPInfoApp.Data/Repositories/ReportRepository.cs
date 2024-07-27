using IPInfoApp.Data.IRepositories;
using IPInfoApp.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Data.Repositories
{
    public class ReportRepository(IConfiguration configuration) : IReportRepository
    {
        private readonly SqlConnection _connection = new(configuration.GetConnectionString("DefaultConnection"));


        /// <summary>
        /// Executed a command to call stored procedure  RPT_CalculateIpAddressesPerCountry to fetch data for reporting according to the given country codes
        /// </summary>
        /// <param name="countryCodeList">The country codes list</param>
        /// <returns></returns>
        public async Task<List<IpAddressPerCountry>> GetIpAddressesPerCountryAsync(string? countryCodeList)
        {
            List<IpAddressPerCountry> results = [];

            using (_connection)
            {
                using var command = new SqlCommand("RPT_CalculateIpAddressesPerCountry", _connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@countryCodeList", string.IsNullOrEmpty(countryCodeList) ? DBNull.Value : countryCodeList);

                await _connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var item = new IpAddressPerCountry
                    {
                        CountryName = reader["CountryName"].ToString() ?? "",
                        AddressesCount = Convert.ToInt32(reader["AddressesCount"]),
                        LastAddressUpdated = Convert.ToDateTime(reader["LastAddressUpdated"])
                    };
                    results.Add(item);
                }
            }

            return results;
        }
    }
}

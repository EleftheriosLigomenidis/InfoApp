using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Exceptions
{
    public class CountryNotFoundException(string ipAddress) : 
        Exception($"Country details for IP address '{ipAddress}' could not be found.")
    {
    }
}

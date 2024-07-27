using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Exceptions
{
    public class IP2CException(HttpStatusCode statusCode, string reasonPhrase, string responseBody) : Exception($"IP2C Web Service Error: {statusCode} - {reasonPhrase}")
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
        public string ReasonPhrase { get; } = reasonPhrase;
        public string ResponseBody { get; } = responseBody;
    }
}

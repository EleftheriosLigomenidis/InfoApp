using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Exceptions
{
    public class ReferentialIntegrityException : Exception
    {
        public ReferentialIntegrityException() { }

        public ReferentialIntegrityException(string message) : base(message) { }

        public ReferentialIntegrityException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

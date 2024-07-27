using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Data.Enums
{
    public enum Datasource
    {
        [Description("Database")]
        Database,
        [Description("Cache")]
        Cache,
        [Description("Web Service")]
        WebService,

    }
}

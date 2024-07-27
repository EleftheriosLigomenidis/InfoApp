using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Options
{
    public class HttpIP2COptions
    {
        public static string Key { get; set; } = "Http";

        [Required]
        public string Uri { get; set; } = null!;
    }
}

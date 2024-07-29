using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Options
{
    public class ApiKeyOptions
    {
        public static string Key { get; set; } = "Auth";
        [Required]
        public string ApiKey { get; set; } = null!;
    }
}

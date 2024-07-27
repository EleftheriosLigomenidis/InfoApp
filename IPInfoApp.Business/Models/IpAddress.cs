using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Models
{
    public record IpAddress
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CountryId { get; set; }
        public virtual Country Country { get; set; } = null!;

        [Required]
        public string Ip { get; set; } = null!;

        public DateTime UpdatedAt { get; set; }

        public IpAddress()
        {}

        public IpAddress(string ipAddress,Country country)
        {
            CreatedAt = DateTime.Now;
            Country = country;
            UpdatedAt = DateTime.Now;
            Ip = ipAddress;
        }
    }
}

using IPInfoApp.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Models
{
    public record Country 
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        [Required]
        [StringLength(3)]
        public string TwoLetterCode { get; set; } = null!;
        [Required]
        [StringLength(2)]
        public string ThreeLetterCode { get; set; } = null!;
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<IpAddress> IpAddresses { get; set; } = new();

        //ToDo:Reduce properties
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPInfoApp.Data.Models;

public partial class Country : Base
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


    public List<IpAddress> IpAddresses { get; set; } = new();
}

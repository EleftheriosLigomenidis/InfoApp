using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPInfoApp.Data.Models;

public partial class IpAddress : Base
{
    public int CountryId { get; set; }
    public virtual Country Country { get; set; } = null!;

    [Required]
    public string Ip { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}

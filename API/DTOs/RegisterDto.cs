using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
     [Required]
     public string Username { get; set; }
     
     [Required]
     [StringLength(8,MinimumLength = 4)]
     public string Password { get; set; }
    }
}
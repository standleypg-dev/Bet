using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using server.Utils;
using server;

namespace server.Models.Dtos;

public class UserDto
{
    [Required]
    public string PhoneNo { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Role { get; set; }
}

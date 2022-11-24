using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;//this will act as a username
    public string Role { get; set; } = string.Empty;
    public bool isVerified { get; set; } = false;

    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenCreated { get; set; } = DateTime.Now;
    public DateTime TokenExpires { get; set; }
}

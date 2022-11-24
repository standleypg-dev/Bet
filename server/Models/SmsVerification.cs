using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models;

public class SmsVerification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; }
    public string Uuid { get; set; }
    public string SmsId { get; set; }
    public User? User { get; set; }
    public string UserId { get; set; }

}

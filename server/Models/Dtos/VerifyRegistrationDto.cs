using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models.Dtos;

public class VerifyRegistrationDto
{
    public string PhoneNo { get; set; }
    public string Code { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Models;

namespace server.Services.iSmsService;

public interface ISmsService
{
    Task<string> SendMsg<T>(T source, RequestType requestType);
    Task<int> AddSmsVerificationAsync(SmsVerification smsVerification);
    Task<SmsVerification> GetSmsVerificationAsync(string PhoneNo);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Utils;

namespace server.Services.iSmsService;

public class SmsService : ISmsService
{
    private readonly DataContext _context;
    public SmsService(DataContext context)
    {
        _context = context;
    }
    public async Task<string> SendMsg<T>(T source, RequestType requestType)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(Util.GetiSmsApiURL(requestType, source)),
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public async Task<int> AddSmsVerificationAsync(SmsVerification smsVerification)
    {
        _context.SmsVerifications.Add(smsVerification);

        return await _context.SaveChangesAsync();
    }
    public async Task<SmsVerification> GetSmsVerificationAsync(string PhoneNo)
    {
        return await _context.SmsVerifications.Include(x => x.User).FirstOrDefaultAsync(x => x.User.PhoneNo == PhoneNo);
    }
}

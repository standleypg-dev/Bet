using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using server.Data;
using server.Models;
using server.Services.iSmsService;
using server.Services.UserService;

namespace server.Helpers;

public class AuthControllerHelper
{
    private readonly IConfiguration _config;
    private readonly ISmsService _smsService;
    private readonly DataContext _context;
    private readonly IUserService _userService;
    public AuthControllerHelper(IConfiguration config, ISmsService smsService, DataContext context, IUserService userService)
    {
        _config = config;
        _smsService = smsService;
        _context = context;
        _userService = userService;
    }
    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public bool VerifyPasswordHash(string password, byte[] passwordhash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordhash);
        }
    }

    public string CreateToken(User user, out DateTime tokenExpired)
    {
        tokenExpired = DateTime.Now.AddHours(1);
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name,user.Name),
            new Claim(ClaimTypes.Role,user.Role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JwtSecret").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: tokenExpired,
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public async Task<bool> SendVerification(string PhoneNo)
    {
        try
        {
            Send send = new()
            {
                CountryCode = PhoneNo,
                Mobile = PhoneNo
            };
            string result = await _smsService.SendMsg(send, RequestType.SEND);
            dynamic res = JsonConvert.DeserializeObject(result);

            string status = res.status;
            if (status.ToLower() == "failed")
                return false;

            var user = await _userService.GetUserAsync(PhoneNo);
            var smsVerification = new SmsVerification()
            {
                Code = res.code,
                Uuid = res.uuid,
                SmsId = res.sms_id,
                User = user
            };
            await _smsService.AddSmsVerificationAsync(smsVerification);

            return true;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> VerifySmsCodeAsync(string PhoneNo, string Code)
    {
        var smsVerification = await _smsService.GetSmsVerificationAsync(PhoneNo);

        var verify = new Verify()
        {
            Mobile = smsVerification.User.PhoneNo,
            CountryCode = smsVerification.User.PhoneNo,
            SmsCode = Code,
            SmsId = smsVerification.SmsId,
            Uuid = smsVerification.Uuid
        };

        string result = await _smsService.SendMsg(verify, RequestType.VERIFY);
        dynamic res = JsonConvert.DeserializeObject(result);
        System.Console.WriteLine(res);
        string status = res.status;
        if (status.ToLower() == "failed")
            return false;

        var user = await _userService.GetUserAsync(PhoneNo);
        await _userService.UpdateUserVerifyStatusAsync(user);
        return true;

    }
}

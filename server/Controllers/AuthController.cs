using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using server.Data;
using server.Helpers;
using server.Models;
using server.Models.Dtos;
using server.Services.iSmsService;
using server.Utils;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DataContext _context;
    private readonly AuthControllerHelper _authControllerHelper;
    private readonly ISmsService _smsService;


    public AuthController(IConfiguration config, DataContext contex, AuthControllerHelper authControllerHelper, ISmsService smsService)
    {
        _config = config;
        _context = contex;
        _authControllerHelper = authControllerHelper;
        _smsService = smsService;
    }
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserDto request)
    {
        _authControllerHelper.CreatePasswordHash(request.Password, out byte[] PasswordHash, out byte[] PasswordSalt);
        var user = new User()
        {
            Name = request.Name,
            PhoneNo = request.PhoneNo,
            PasswordHash = PasswordHash,
            PasswordSalt = PasswordSalt,
            Role = request?.Role?.Trim().ToLower() == Util.GetRole(Role.ADMIN) ? request.Role.Trim().ToLower() : Util.GetRole(Role.USER)
        };

        if (await _context.Users.FirstOrDefaultAsync(x => x.PhoneNo == request.PhoneNo) is not null ? true : false)
        {
            return StatusCode(409, HttpStatusCode.Conflict);
        }
        _context.Users.Add(user);

        if (await _context.SaveChangesAsync() != 0)
        {
            if (!await _authControllerHelper.SendVerification(request.PhoneNo))
            {
                user = await _context.Users.FindAsync(user.Id);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                var smsVerification = await _context.SmsVerifications.FirstOrDefaultAsync(x => x.UserId == user.Id);
                smsVerification = await _context.SmsVerifications.FindAsync(smsVerification.Id);
                _context.SmsVerifications.Remove(smsVerification);
                await _context.SaveChangesAsync();

                return BadRequest("Register failed");
            }

            return Ok(user);
        }

        return BadRequest();
    }

    [HttpPost("verify")]
    public async Task<ActionResult> VerifyRegistration(VerifyRegistrationDto verify)
    {
        if (await _authControllerHelper.VerifySmsCodeAsync(verify.PhoneNo, verify.Code))
            return Ok();

        return BadRequest();
    }

    [HttpPost("resend_code")]
    public async Task<ActionResult> ResendCode(string PhoneNo)
    {

        var user = await _context.Users.FirstOrDefaultAsync(x => x.PhoneNo == PhoneNo);
        user = await _context.Users.FindAsync(user.Id);
        var sms = await _context.SmsVerifications.FirstOrDefaultAsync(x => x.User.PhoneNo == PhoneNo);
        var v = await _context.SmsVerifications.FindAsync(sms.Id);
        System.Console.WriteLine(v.Code);
        Send send = new()
        {
            CountryCode = PhoneNo,
            Mobile = PhoneNo
        };
        string result = await _smsService.SendMsg(send, RequestType.SEND);
        dynamic res = JsonConvert.DeserializeObject(result);
        System.Console.WriteLine(res);
        v.Code = res.code;
        v.Uuid = res.uuid;
        v.SmsId = res.sms_id;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginDto>> Login(UserDto request)
    {
        //get the user from DB
        var user = await _context.Users.FirstOrDefaultAsync(x => x.PhoneNo == request.PhoneNo);
        var Login = new LoginDto();
        if (user is not null)
        {
            if (!user.isVerified)
                return BadRequest("Account not verified");
            if (!_authControllerHelper.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                return BadRequest("Wrong Password.");

            Login.Token = _authControllerHelper.CreateToken(user, out DateTime tokenExpired);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken, ref user);
            Login.Role = user.Role;
            Login.name = user.Name;

            //Update Database token time
            await _context.SaveChangesAsync();
            return Ok(Login);
        }
        else
            return NotFound();

    }

    [HttpGet("refresh-token"), Authorize]
    public async Task<ActionResult<string>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

        if (user is null)
            return Unauthorized("Invalid Refresh Token.");

        if (user.TokenExpires < DateTime.Now)
            return Unauthorized("Token expired");

        string token = _authControllerHelper.CreateToken(user, out DateTime tokenExpired);
        var newRefreshToken = GenerateRefreshToken();
        SetRefreshToken(newRefreshToken, ref user);

        await _context.SaveChangesAsync();


        return Ok(token);
    }

    [HttpPost("revoke/{PhoneNo}")]
    public async Task<ActionResult> Revoke(string PhoneNo)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.PhoneNo == PhoneNo);
        if (user is null)
            return BadRequest("Invalid Phone Number");

        user.RefreshToken = string.Empty;
        user = await _context.Users.FindAsync(user.Id);
        await _context.SaveChangesAsync();

        return Ok();

    }

    private RefreshTokenDto GenerateRefreshToken()
    {
        var refreshToken = new RefreshTokenDto
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.Now.AddHours(1),
            Created = DateTime.Now
        };
        return refreshToken;
    }

    private void SetRefreshToken(RefreshTokenDto newRefreshToken, ref User user)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = newRefreshToken.Expires
        };
        Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

        user.RefreshToken = newRefreshToken.Token;
        user.TokenCreated = newRefreshToken.Created;
        user.TokenExpires = newRefreshToken.Expires;
    }

}

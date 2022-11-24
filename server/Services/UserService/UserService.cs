using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services.UserService;

public class UserService : IUserService
{
    private readonly DataContext _context;
    public UserService(DataContext context)
    {
        _context = context;
    }

    public async Task<User> GetUserAsync(string Mobile)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.PhoneNo == Mobile);
    }
    public async Task UpdateUserVerifyStatusAsync(User user)
    {
        var res = await _context.Users.FirstOrDefaultAsync(x => x.PhoneNo == user.PhoneNo);
        var dbUser = await _context.Users.FindAsync(res.Id);
        dbUser.isVerified = true;
        await _context.SaveChangesAsync();
    }
}

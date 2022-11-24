using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Models;

namespace server.Services.UserService;

public interface IUserService
{
    Task<User> GetUserAsync(string Mobile);
    Task UpdateUserVerifyStatusAsync(User user);
}

using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Models;

namespace rentalApp.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetUserByEmail(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }
}
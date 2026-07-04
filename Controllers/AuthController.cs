using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Extensions;
using rentalApp.Models;
using rentalApp.Models.Dtos;
using rentalApp.Models.Enum;
using rentalApp.Services;

namespace RentalApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var exists = await _context.Users.AnyAsync(x => x.Email == dto.Email);

        if (exists)
            return Problem(detail: "Email already exists", statusCode: StatusCodes.Status400BadRequest);

        var requestedRole = string.IsNullOrWhiteSpace(dto.Role) ? UserRoles.Guest.ToString() : dto.Role;

        if (!Enum.TryParse<UserRoles>(requestedRole, ignoreCase: true, out var parsedRole)
            || parsedRole == UserRoles.Admin)
        {
            return Problem(detail: "Role must be Guest or Owner", statusCode: StatusCodes.Status400BadRequest);
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = parsedRole.ToString()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.Role
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Problem(detail: "Invalid credentials", statusCode: StatusCodes.Status401Unauthorized);

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            expiresAt,
            user = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Problem(detail: "User not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.Role
        });
    }
}

using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BC = BCrypt.Net.BCrypt;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register(RegisterDto registerDto)
        {
        try
        {
        // Check if user with email already exists
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            return BadRequest(new { success = false, message = "Email is already registered" });
        }

        // Check if username is taken
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
        {
            return BadRequest(new { success = false, message = "Username is already taken" });
        }

        // Create new user with hashed password
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            Password = BC.HashPassword(registerDto.Password),
            Course = registerDto.Course,
            YearOfStudy = registerDto.YearOfStudy,
            AccommodationType = registerDto.AccommodationType,
            JoinDate = DateTime.UtcNow,
            LastActivityDate = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Return success without password
        return new { 
            success = true, 
            userId = user.Id, 
            username = user.Username 
        };
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Registration error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        
        // Return a proper JSON error response instead of letting the framework
        // handle the exception, which might return HTML
        return StatusCode(500, new { success = false, message = "An error occurred during registration" });
    }
}

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            // Verify password (assumes password is hashed in the database)
            bool validPassword = BC.Verify(loginDto.Password, user.Password);

            if (!validPassword)
            {
                return Unauthorized("Invalid email or password");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new
            {
                token,
                userId = user.Id,
                username = user.Username
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? 
                throw new InvalidOperationException("JWT key is not configured"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    // DTOs for authentication
    public class RegisterDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required]
    public string Course { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 6)]
    public int YearOfStudy { get; set; } = 1;
    
    [Required]
    public string AccommodationType { get; set; } = string.Empty;
}

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
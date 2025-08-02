using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Server.Services.Interfaces;
using BC = BCrypt.Net.BCrypt;
using Server.DTOs;

namespace Server.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message, int? UserId, string? Username)> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return (false, "Email is already registered", null, null);

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                return (false, "Username is already taken", null, null);

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

            if (_userManager != null)
            {
                var identityUser = new IdentityUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(identityUser, registerDto.Password);
                // Optionally handle result
            }

            return (true, "Registration successful", user.Id, user.Username);
        }

        public async Task<(bool Success, string Message, string? Token, int? UserId, string? Username, string? Email, IList<string>? Roles)> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
                return (false, "Invalid email or password", null, null, null, null, null);

            bool validPassword = false;
            try
            {
                validPassword = BC.Verify(loginDto.Password, user.Password);
            }
            catch
            {
                if (loginDto.Password == user.Password)
                {
                    user.Password = BC.HashPassword(loginDto.Password);
                    await _context.SaveChangesAsync();
                    validPassword = true;
                }
            }

            if (!validPassword)
                return (false, "Invalid email or password", null, null, null, null, null);

            var roles = await GetUserRolesAsync(user.Email);
            var token = await GenerateJwtTokenAsync(user, roles);

            return (true, "Login successful", token, user.Id, user.Username, user.Email, roles);
        }

        public async Task<object?> GetUserInfoAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;
            var roles = await GetUserRolesAsync(user.Email);

            return new
            {
                userId = user.Id,
                username = user.Username,
                email = user.Email,
                course = user.Course,
                yearOfStudy = user.YearOfStudy,
                accommodationType = user.AccommodationType,
                points = user.Points,
                currentStreak = user.CurrentStreak,
                maxStreak = user.MaxStreak,
                joinDate = user.JoinDate,
                lastActivityDate = user.LastActivityDate,
                roles = roles
            };
        }

        public async Task<(bool Success, object? TokenInfo)> ValidateTokenAsync(ClaimsPrincipal user)
        {
            try
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                var userNameClaim = user.FindFirst(ClaimTypes.Name);
                var userEmailClaim = user.FindFirst(ClaimTypes.Email);

                var roles = user.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                return (true, new
                {
                    userId = userIdClaim?.Value,
                    username = userNameClaim?.Value,
                    email = userEmailClaim?.Value,
                    roles = roles,
                    isAdmin = user.IsInRole("Admin")
                });
            }
            catch
            {
                return (false, null);
            }
        }

        public async Task<IList<string>> GetUserRolesAsync(string email)
        {
            var userRoles = new List<string>();
            if (_userManager != null)
            {
                var identityUser = await _userManager.FindByEmailAsync(email);
                if (identityUser != null)
                {
                    userRoles = (await _userManager.GetRolesAsync(identityUser)).ToList();
                }
            }
            // Optionally add direct DB lookup fallback here if needed
            return userRoles;
        }

        public async Task<string> GenerateJwtTokenAsync(User user, IList<string> roles)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
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
}
using Server.Data;
using Server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Server.Services.Interfaces;
using Server.DTOs;
using Server.Repositories.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace Server.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthService(
            IUserRepository userRepository,
            AppDbContext context,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            _userRepository = userRepository;
            _context = context;
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message, int? UserId, string? Username)> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userRepository.GetByEmailAsync(registerDto.Email) != null)
                return (false, "Email is already registered", null, null);

            if ((await _userRepository.GetAllAsync()).Any(u => u.Username == registerDto.Username))
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

            await _userRepository.AddAsync(user);

            // Also create IdentityUser for roles
            var identityUser = new IdentityUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(identityUser, registerDto.Password);

            return (true, "Registration successful", user.Id, user.Username);
        }

        public async Task<(bool Success, string Message, string? Token, string? RefreshToken, int? UserId, string? Username, string? Email, IList<string>? Roles)> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null)
                return (false, "Invalid email or password", null, null, null, null, null, null);

            if (!BC.Verify(loginDto.Password, user.Password))
                return (false, "Invalid email or password", null, null, null, null, null, null);

            var roles = await GetUserRolesAsync(user.Email);

            var jwtToken = await GenerateJwtTokenAsync(user, roles);
            var refreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            return (true, "Login successful", jwtToken, refreshToken, user.Id, user.Username, user.Email, roles);
        }

        public async Task<object?> GetUserInfoAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
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
                roles
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
                    roles,
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
            var identityUser = await _userManager.FindByEmailAsync(email);
            return identityUser != null ? await _userManager.GetRolesAsync(identityUser) : new List<string>();
        }

        public async Task<string> GenerateJwtTokenAsync(User user, IList<string> roles)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<(bool Success, string? NewJwt, string? NewRefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

            if (storedToken == null || storedToken.Expires < DateTime.UtcNow)
                return (false, null, null);

            var user = await _context.Users.FindAsync(storedToken.UserId);
            if (user == null)
                return (false, null, null);

            var roles = await GetUserRolesAsync(user.Email);
            var newJwt = await GenerateJwtTokenAsync(user, roles);

            storedToken.IsRevoked = true;

            var newRefreshToken = GenerateRefreshToken();
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            return (true, newJwt, newRefreshToken);
        }
    }
}

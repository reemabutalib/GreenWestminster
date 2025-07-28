using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BC = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(
            AppDbContext context, 
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager = null,
            UserManager<IdentityUser> userManager = null)
        {
            _context = context;
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = userManager;
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
                    // Use a more explicit BCrypt hash method that won't cause issues
                    Password = BC.HashPassword(registerDto.Password),
                    Course = registerDto.Course,
                    YearOfStudy = registerDto.YearOfStudy,
                    AccommodationType = registerDto.AccommodationType,
                    JoinDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create corresponding identity user if we have identity services
                if (_userManager != null)
                {
                    var identityUser = new IdentityUser
                    {
                        UserName = registerDto.Email,
                        Email = registerDto.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(identityUser, registerDto.Password);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Created identity user for {registerDto.Email}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create identity user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

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
            try 
            {
                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Invalid email or password" });
                }

                bool validPassword = false;
                
                try 
                {
                    // Try standard verification
                    validPassword = BC.Verify(loginDto.Password, user.Password);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // If we get here, the password might be stored in plaintext or another format
                    Console.WriteLine("BCrypt verification failed - password might not be hashed properly");
                    
                    // TEMPORARY SOLUTION: For users with invalid hash format, 
                    // check if password matches directly (for migration)
                    if (loginDto.Password == user.Password)
                    {
                        // Update the password with a proper hash
                        user.Password = BC.HashPassword(loginDto.Password);
                        await _context.SaveChangesAsync();
                        validPassword = true;
                    }
                }

                if (!validPassword)
                {
                    return Unauthorized(new { success = false, message = "Invalid email or password" });
                }

                // Generate JWT token
                var token = await GenerateJwtToken(user);

                // Get user roles for response
                var userRoles = await GetUserRoles(user.Email);

                return new
                {
                    success = true,
                    token,
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    roles = userRoles
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        // GET: api/auth/user-info
        [HttpGet("user-info")]
        public async Task<ActionResult> GetUserInfo()
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Unable to identify user" });
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Get user roles
                var userRoles = await GetUserRoles(user.Email);

                return Ok(new
                {
                    success = true,
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
                    roles = userRoles
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user info: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving user information" });
            }
        }

        // POST: api/auth/validate-token
        [HttpPost("validate-token")]
        public ActionResult ValidateToken()
        {
            try
            {
                // If we get here, the token is valid (the [Authorize] would have failed otherwise)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(ClaimTypes.Name);
                var userEmailClaim = User.FindFirst(ClaimTypes.Email);
                
                var roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    userId = userIdClaim?.Value,
                    username = userNameClaim?.Value,
                    email = userEmailClaim?.Value,
                    roles = roles,
                    isAdmin = User.IsInRole("Admin")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred during token validation" });
            }
        }

        // Helper method to get user roles
        private async Task<List<string>> GetUserRoles(string email)
        {
            var userRoles = new List<string>();
            
            try
            {
                // Try to get roles from ASP.NET Identity first
                if (_userManager != null)
                {
                    var identityUser = await _userManager.FindByEmailAsync(email);
                    if (identityUser != null)
                    {
                        userRoles = (await _userManager.GetRolesAsync(identityUser)).ToList();
                        Console.WriteLine($"Found {userRoles.Count} roles for user {email} from ASP.NET Identity");
                    }
                }
                
                // If no roles found or identity services not available, try direct database access
                if (userRoles.Count == 0 && _context.Database.CanConnect())
                {
                    using (var connection = _context.Database.GetDbConnection())
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                            connection.Open();
                            
                        using (var command = connection.CreateCommand())
                        {
                            // Check if AspNetUserRoles table exists
                            try
                            {
                                command.CommandText = @"
                                    SELECT EXISTS (
                                        SELECT FROM information_schema.tables 
                                        WHERE table_name = 'aspnetuserroles'
                                    )";
                                
                                bool tableExists = (bool)await command.ExecuteScalarAsync();
                                
                                if (tableExists)
                                {
                                    command.CommandText = @"
                                        SELECT r.Name
                                        FROM AspNetRoles r
                                        JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
                                        JOIN AspNetUsers u ON ur.UserId = u.Id
                                        WHERE u.Email = @Email";
                                    
                                    var parameter = command.CreateParameter();
                                    parameter.ParameterName = "Email";
                                    parameter.Value = email;
                                    command.Parameters.Add(parameter);
                                    
                                    using (var reader = await command.ExecuteReaderAsync())
                                    {
                                        while (await reader.ReadAsync())
                                        {
                                            userRoles.Add(reader.GetString(0));
                                        }
                                    }
                                    
                                    Console.WriteLine($"Found {userRoles.Count} roles for user {email} from direct database query");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error checking user roles from database: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user roles: {ex.Message}");
            }
            
            return userRoles;
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? 
                throw new InvalidOperationException("JWT key is not configured"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            
            // Add role claims
            var userRoles = await GetUserRoles(user.Email);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                Console.WriteLine($"Added role claim: {role} for user {user.Email}");
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
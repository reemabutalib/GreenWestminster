using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Server.Services.Interfaces;
using Server.DTOs;
using Server.Data;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register(RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return new
                {
                    success = true,
                    userId = result.UserId,
                    username = result.Username
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                if (!result.Success)
                    return Unauthorized(new { success = false, message = result.Message });

                return new
                {
                    success = true,
                    token = result.Token,
                    userId = result.UserId,
                    username = result.Username,
                    email = result.Email,
                    roles = result.Roles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        // GET: api/auth/user-info
        [HttpGet("user-info")]
        public async Task<ActionResult> GetUserInfo()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(new { success = false, message = "Unable to identify user" });

                var userInfo = await _authService.GetUserInfoAsync(userId);
                if (userInfo == null)
                    return NotFound(new { success = false, message = "User not found" });

                return Ok(new { success = true, userInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving user information" });
            }
        }

        // POST: api/auth/validate-token
        [HttpPost("validate-token")]
        public ActionResult ValidateToken()
        {
            try
            {
                var result = _authService.ValidateTokenAsync(User).Result;
                if (!result.Success)
                    return Unauthorized(new { success = false, message = "Invalid token" });

                return Ok(new { success = true, tokenInfo = result.TokenInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation error");
                return StatusCode(500, new { success = false, message = "An error occurred during token validation" });
            }
        }
    }
}
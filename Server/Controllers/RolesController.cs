using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Models;
using Microsoft.AspNetCore.Authorization;
using Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        
        public RolesController(
            RoleManager<IdentityRole> roleManager, 
            UserManager<IdentityUser> userManager,
            AppDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        
        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }
        
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }
            return Ok(role);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return Ok("Role created successfully.");
            }
            return BadRequest(result.Errors);
        }
        
        [HttpPut]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }
            role.Name = model.NewRoleName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return Ok("Role updated successfully.");
            }
            return BadRequest(result.Errors);
        }
        
        [HttpDelete]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Ok("Role deleted successfully.");
            }
            return BadRequest(result.Errors);
        }
        
        [HttpPost("assign-role-to-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
            {
                return NotFound("Role not found.");
            }
            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (result.Succeeded)
            {
                return Ok("Role assigned to user successfully.");
            }
            return BadRequest(result.Errors);
        }
        
        // New endpoint to get all identity users
        [HttpGet("identity-users")]
        public IActionResult GetIdentityUsers()
        {
            var users = _userManager.Users.Select(u => new { u.Id, u.UserName, u.Email }).ToList();
            return Ok(users);
        }
        
        // New endpoint to create an identity user
        [HttpPost("create-identity-user")]
        public async Task<IActionResult> CreateIdentityUser([FromBody] CreateIdentityUserModel model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                return Ok(new { Id = user.Id, Email = user.Email });
            }
            
            return BadRequest(result.Errors);
        }
        
        // New endpoint to assign role to custom user by email
        [HttpPost("assign-role-by-email")]
        public async Task<IActionResult> AssignRoleByEmail([FromBody] AssignRoleByEmailModel model)
        {
            var identityUser = await _userManager.FindByEmailAsync(model.Email);
            if (identityUser == null)
            {
                return NotFound($"Identity user with email {model.Email} not found.");
            }
            
            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
            {
                return NotFound($"Role '{model.RoleName}' not found.");
            }
            
            var result = await _userManager.AddToRoleAsync(identityUser, model.RoleName);
            if (result.Succeeded)
            {
                return Ok($"Role '{model.RoleName}' assigned to user with email '{model.Email}' successfully.");
            }
            
            return BadRequest(result.Errors);
        }
        
        // New endpoint to assign role to custom user by ID
        [HttpPost("assign-role-to-custom-user")]
        public async Task<IActionResult> AssignRoleToCustomUser([FromBody] AssignCustomRoleModel model)
        {
            // First find your custom user
            var customUser = await _context.Users.FindAsync(model.CustomUserId);
            if (customUser == null)
            {
                return NotFound($"Custom user with ID {model.CustomUserId} not found.");
            }
            
            // Check if an Identity user with this email exists
            var identityUser = await _userManager.FindByEmailAsync(customUser.Email);
            
            // If not, create one
            if (identityUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = customUser.Email,
                    Email = customUser.Email,
                    EmailConfirmed = true
                };
                
                // Generate a secure random password for the identity user
                var password = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 16) + "!1Aa";
                var createResult = await _userManager.CreateAsync(identityUser, password);
                
                if (!createResult.Succeeded)
                {
                    return BadRequest("Failed to create identity user: " + 
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
            
            // Now assign the role
            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
            {
                return NotFound($"Role '{model.RoleName}' not found.");
            }
            
            var result = await _userManager.AddToRoleAsync(identityUser, model.RoleName);
            if (result.Succeeded)
            {
                return Ok($"Role '{model.RoleName}' assigned to user '{customUser.Username}' successfully.");
            }
            
            return BadRequest(result.Errors);
        }
        
        // New endpoint to check user roles
        [HttpGet("user-roles/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { UserId = userId, Roles = roles });
        }
    }
    
    public class CreateIdentityUserModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    
    public class AssignRoleByEmailModel
    {
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
    
    public class AssignCustomRoleModel
    {
        public int CustomUserId { get; set; }
        public string RoleName { get; set; }
    }
}
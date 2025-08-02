using Microsoft.AspNetCore.Identity;
using Server.Data;
using Server.Models;
using Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.DTOs;

namespace Server.Services.Implementations
{
    public class RolesService : IRolesService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public RolesService(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            AppDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public List<IdentityRole> GetRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public async Task<IdentityRole?> GetRoleAsync(string roleId)
        {
            return await _roleManager.FindByIdAsync(roleId);
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            var role = new IdentityRole(roleName);
            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(UpdateRoleModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            role.Name = model.NewRoleName;
            return await _roleManager.UpdateAsync(role);
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<IdentityResult> AssignRoleToUserAsync(AssignRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            return await _userManager.AddToRoleAsync(user, model.RoleName);
        }

        public List<object> GetIdentityUsers()
        {
            return _userManager.Users.Select(u => new { u.Id, u.UserName, u.Email }).ToList<object>();
        }

        public async Task<(bool Success, object? Result)> CreateIdentityUserAsync(CreateIdentityUserModel model)
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
                return (true, new { Id = user.Id, Email = user.Email });
            }

            return (false, result.Errors);
        }

        public async Task<IdentityResult> AssignRoleByEmailAsync(AssignRoleByEmailModel model)
        {
            var identityUser = await _userManager.FindByEmailAsync(model.Email);
            if (identityUser == null)
                return IdentityResult.Failed(new IdentityError { Description = $"Identity user with email {model.Email} not found." });

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
                return IdentityResult.Failed(new IdentityError { Description = $"Role '{model.RoleName}' not found." });

            return await _userManager.AddToRoleAsync(identityUser, model.RoleName);
        }

        public async Task<(bool Success, string? Message)> AssignRoleToCustomUserAsync(AssignCustomRoleModel model)
        {
            var customUser = await _context.Users.FindAsync(model.CustomUserId);
            if (customUser == null)
                return (false, $"Custom user with ID {model.CustomUserId} not found.");

            var identityUser = await _userManager.FindByEmailAsync(customUser.Email);

            if (identityUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = customUser.Email,
                    Email = customUser.Email,
                    EmailConfirmed = true
                };

                var password = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 16) + "!1Aa";
                var createResult = await _userManager.CreateAsync(identityUser, password);

                if (!createResult.Succeeded)
                {
                    return (false, "Failed to create identity user: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
                return (false, $"Role '{model.RoleName}' not found.");

            var result = await _userManager.AddToRoleAsync(identityUser, model.RoleName);
            if (result.Succeeded)
            {
                return (true, $"Role '{model.RoleName}' assigned to user '{customUser.Username}' successfully.");
            }

            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }
    }
}
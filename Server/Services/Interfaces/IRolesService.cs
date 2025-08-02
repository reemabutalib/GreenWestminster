using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;
using Server.DTOs;

namespace Server.Services.Interfaces
{
    public interface IRolesService
    {
        List<IdentityRole> GetRoles();
        Task<IdentityRole?> GetRoleAsync(string roleId);
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<IdentityResult> UpdateRoleAsync(UpdateRoleModel model);
        Task<IdentityResult> DeleteRoleAsync(string roleId);
        Task<IdentityResult> AssignRoleToUserAsync(AssignRoleModel model);
        List<object> GetIdentityUsers();
        Task<(bool Success, object? Result)> CreateIdentityUserAsync(CreateIdentityUserModel model);
        Task<IdentityResult> AssignRoleByEmailAsync(AssignRoleByEmailModel model);
        Task<(bool Success, string? Message)> AssignRoleToCustomUserAsync(AssignCustomRoleModel model);
        Task<IList<string>> GetUserRolesAsync(string userId);
    }
}
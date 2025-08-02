using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Server.Models;
using Microsoft.AspNetCore.Authorization;
using Server.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _rolesService.GetRoles();
            return Ok(roles);
        }

        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRole(string roleId)
        {
            var role = await _rolesService.GetRoleAsync(roleId);
            if (role == null)
                return NotFound("Role not found.");
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var result = await _rolesService.CreateRoleAsync(roleName);
            if (result.Succeeded)
                return Ok("Role created successfully.");
            return BadRequest(result.Errors);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleModel model)
        {
            var result = await _rolesService.UpdateRoleAsync(model);
            if (result.Succeeded)
                return Ok("Role updated successfully.");
            return BadRequest(result.Errors);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var result = await _rolesService.DeleteRoleAsync(roleId);
            if (result.Succeeded)
                return Ok("Role deleted successfully.");
            return BadRequest(result.Errors);
        }

        [HttpPost("assign-role-to-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleModel model)
        {
            var result = await _rolesService.AssignRoleToUserAsync(model);
            if (result.Succeeded)
                return Ok("Role assigned to user successfully.");
            return BadRequest(result.Errors);
        }

        [HttpGet("identity-users")]
        public IActionResult GetIdentityUsers()
        {
            var users = _rolesService.GetIdentityUsers();
            return Ok(users);
        }

        [HttpPost("create-identity-user")]
        public async Task<IActionResult> CreateIdentityUser([FromBody] CreateIdentityUserModel model)
        {
            var (success, result) = await _rolesService.CreateIdentityUserAsync(model);
            if (success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("assign-role-by-email")]
        public async Task<IActionResult> AssignRoleByEmail([FromBody] AssignRoleByEmailModel model)
        {
            var result = await _rolesService.AssignRoleByEmailAsync(model);
            if (result.Succeeded)
                return Ok($"Role '{model.RoleName}' assigned to user with email '{model.Email}' successfully.");
            return BadRequest(result.Errors);
        }

        [HttpPost("assign-role-to-custom-user")]
        public async Task<IActionResult> AssignRoleToCustomUser([FromBody] AssignCustomRoleModel model)
        {
            var (success, message) = await _rolesService.AssignRoleToCustomUserAsync(model);
            if (success)
                return Ok(message);
            return BadRequest(message);
        }

        [HttpGet("user-roles/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _rolesService.GetUserRolesAsync(userId);
            return Ok(new { UserId = userId, Roles = roles });
        }
    }
}
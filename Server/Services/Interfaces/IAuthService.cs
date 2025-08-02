using System.Threading.Tasks;
using Server.Models;
using System.Security.Claims;
using Server.DTOs;

namespace Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, int? UserId, string? Username)> RegisterAsync(RegisterDto registerDto);
        Task<(bool Success, string Message, string? Token, int? UserId, string? Username, string? Email, IList<string>? Roles)> LoginAsync(LoginDto loginDto);
        Task<object?> GetUserInfoAsync(int userId);
        Task<(bool Success, object? TokenInfo)> ValidateTokenAsync(ClaimsPrincipal user);
        Task<IList<string>> GetUserRolesAsync(string email);
        Task<string> GenerateJwtTokenAsync(User user, IList<string> roles);
    }
}
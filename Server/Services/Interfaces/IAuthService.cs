using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Server.DTOs;
using Server.Models;

namespace Server.Services.Interfaces
{
    public interface IAuthService
{
    Task<(bool Success, string Message, string? Token, string? RefreshToken, int? UserId, string? Username, string? Email, IList<string>? Roles)> LoginAsync(LoginDto loginDto);
    Task<(bool Success, string Message, int? UserId, string? Username)> RegisterAsync(RegisterDto registerDto);
    Task<object?> GetUserInfoAsync(int userId);
    Task<(bool Success, object? TokenInfo)> ValidateTokenAsync(ClaimsPrincipal user);
    Task<IList<string>> GetUserRolesAsync(string email);
    Task<string> GenerateJwtTokenAsync(User user, IList<string> roles);
    string GenerateRefreshToken();
    Task<(bool Success, string? NewJwt, string? NewRefreshToken)> RefreshTokenAsync(string refreshToken);
}

}

using DocApi.DTOs;

namespace DocApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null);  // ← ipAddress ajouté
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null);  // ← NOUVEAU
        Task LogoutAsync(string refreshToken);                                          // ← NOUVEAU
        Task<UserResponse> GetUserProfileAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);      // ← NOUVEAU
        string GenerateJwtToken(int userId, string username, string role);
    }
}
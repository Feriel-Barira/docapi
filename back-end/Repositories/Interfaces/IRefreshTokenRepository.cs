using DocApi.Domain.Entities;

namespace DocApi.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task CreateAsync(RefreshToken refreshToken);
        Task RevokeAsync(string token);
        Task RevokeAllForUserAsync(int userId);
        Task CleanExpiredAsync();
    }
}
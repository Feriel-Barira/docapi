using Dapper;
using DocApi.Domain.Entities;
using DocApi.Infrastructure;
using DocApi.Repositories.Interfaces;

namespace DocApi.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnectionFactory _factory;

        public RefreshTokenRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                SELECT Id, UserId, Token, ExpiresAt, IsRevoked, CreatedAt, CreatedByIp
                FROM RefreshTokens WHERE Token = @Token";
            return await conn.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task CreateAsync(RefreshToken refreshToken)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, IsRevoked, CreatedAt, CreatedByIp)
                VALUES (@UserId, @Token, @ExpiresAt, @IsRevoked, @CreatedAt, @CreatedByIp)";
            await conn.ExecuteAsync(sql, refreshToken);
        }

        public async Task RevokeAsync(string token)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "UPDATE RefreshTokens SET IsRevoked = TRUE WHERE Token = @Token";
            await conn.ExecuteAsync(sql, new { Token = token });
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = "UPDATE RefreshTokens SET IsRevoked = TRUE WHERE UserId = @UserId";
            await conn.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task CleanExpiredAsync()
        {
            using var conn = _factory.CreateConnection();
            const string sql = "DELETE FROM RefreshTokens WHERE ExpiresAt < NOW() OR IsRevoked = TRUE";
            await conn.ExecuteAsync(sql);
        }
    }
}
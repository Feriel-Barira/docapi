using Dapper;
using DocApi.Domain.Entities;
using DocApi.Repositories.Interfaces;
using System.Data;

namespace DocApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;

        // ── SELECT complet avec nouveaux champs ───────────────
        private const string SELECT_ALL = @"
            SELECT Id, Username, Email, PasswordHash, 
                   Role, RoleGlobal, CAST(OrganisationId AS CHAR) AS OrganisationId,
                   Nom, Prenom, Fonction,
                   CreatedAt, IsActive 
            FROM Users";

        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            var sql = $"{SELECT_ALL} WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var sql = $"{SELECT_ALL} WHERE Username = @Username";
            return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var sql = $"{SELECT_ALL} WHERE Email = @Email";
            return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var sql = $"{SELECT_ALL} WHERE Username = @Value OR Email = @Value";
            return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Value = usernameOrEmail });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var sql = $"{SELECT_ALL} ORDER BY Id";
            return await _connection.QueryAsync<User>(sql);
        }

        public async Task<int> CreateAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users (Username, Email, PasswordHash, Role, RoleGlobal, OrganisationId, Nom, Prenom, Fonction, CreatedAt, IsActive)
                VALUES (@Username, @Email, @PasswordHash, @Role, @RoleGlobal, @OrganisationId, @Nom, @Prenom, @Fonction, @CreatedAt, @IsActive);
                SELECT LAST_INSERT_ID();";

            user.CreatedAt = DateTime.UtcNow;
            return await _connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE Users 
                SET Username      = @Username,
                    Email         = @Email,
                    PasswordHash  = @PasswordHash,
                    Role          = @Role,
                    RoleGlobal    = @RoleGlobal,
                    OrganisationId = @OrganisationId,
                    Nom           = @Nom,
                    Prenom        = @Prenom,
                    Fonction      = @Fonction,
                    IsActive      = @IsActive
                WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, user);
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Users WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }

        public async Task<bool> ExistsAsync(string username, string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username OR Email = @Email";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Username = username, Email = email });
            return count > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Id = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
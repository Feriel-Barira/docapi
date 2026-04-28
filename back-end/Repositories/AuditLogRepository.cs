using Dapper;
using DocApi.Domain.Entities;
using DocApi.DTOs;
using DocApi.Infrastructure;
using DocApi.Repositories.Interfaces;

namespace DocApi.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IDbConnectionFactory _factory;

        public AuditLogRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task LogAsync(AuditLog log)
        {
            using var conn = _factory.CreateConnection();
            const string sql = @"
                INSERT INTO AuditLogs (UserId, Username, Action, EntityType, EntityId, Details, IpAddress, DateAction)
                VALUES (@UserId, @Username, @Action, @EntityType, @EntityId, @Details, @IpAddress, @DateAction)";
            await conn.ExecuteAsync(sql, log);
        }

        public async Task<PagedResultDto<AuditLogDto>> GetFilteredAsync(AuditLogFilterDto filter)
        {
            using var conn = _factory.CreateConnection();
            var where = new List<string>();
            var p = new DynamicParameters();

            if (filter.UserId.HasValue)      { where.Add("UserId = @UserId");           p.Add("UserId", filter.UserId.Value); }
            if (!string.IsNullOrWhiteSpace(filter.Action))     { where.Add("Action = @Action");           p.Add("Action", filter.Action); }
            if (!string.IsNullOrWhiteSpace(filter.EntityType)) { where.Add("EntityType = @EntityType");   p.Add("EntityType", filter.EntityType); }
            if (filter.DateDebut.HasValue)   { where.Add("DateAction >= @DateDebut");   p.Add("DateDebut", filter.DateDebut.Value); }
            if (filter.DateFin.HasValue)     { where.Add("DateAction <= @DateFin");     p.Add("DateFin", filter.DateFin.Value); }

            var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

            var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM AuditLogs {whereClause}", p);

            p.Add("Offset", (filter.Page - 1) * filter.PageSize);
            p.Add("PageSize", filter.PageSize);

            var data = await conn.QueryAsync<AuditLogDto>($@"
                SELECT Id, UserId, Username, Action, EntityType, EntityId, Details, IpAddress, DateAction
                FROM AuditLogs {whereClause}
                ORDER BY DateAction DESC
                LIMIT @Offset, @PageSize", p);

            return new PagedResultDto<AuditLogDto>
            {
                Data = data.ToList(),
                Total = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<IEnumerable<AuditLogDto>> GetRecentAsync(int count = 20)
        {
            using var conn = _factory.CreateConnection();
            return await conn.QueryAsync<AuditLogDto>(@"
                SELECT Id, UserId, Username, Action, EntityType, EntityId, Details, IpAddress, DateAction
                FROM AuditLogs ORDER BY DateAction DESC LIMIT @Count", new { Count = count });
        }
    }
}
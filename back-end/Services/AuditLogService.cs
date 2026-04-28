using System.Security.Claims;
using DocApi.Domain.Entities;
using DocApi.DTOs;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;

namespace DocApi.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;
        private readonly IHttpContextAccessor _http;

        public AuditLogService(IAuditLogRepository repo, IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        public async Task LogAsync(string action, string entityType, string? entityId, string? details = null)
        {
            var ctx = _http.HttpContext;
            int? userId = null;
            string username = "Système";

            if (ctx?.User?.Identity?.IsAuthenticated == true)
            {
                var claim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(claim, out var uid)) userId = uid;
                username = ctx.User.FindFirst(ClaimTypes.Name)?.Value ?? "Inconnu";
            }

            await _repo.LogAsync(new AuditLog
            {
                UserId = userId,
                Username = username,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                DateAction = DateTime.UtcNow
            });
        }

        public Task<PagedResultDto<AuditLogDto>> GetFilteredAsync(AuditLogFilterDto filter)
            => _repo.GetFilteredAsync(filter);

        public Task<IEnumerable<AuditLogDto>> GetRecentAsync(int count = 20)
            => _repo.GetRecentAsync(count);
    }
}

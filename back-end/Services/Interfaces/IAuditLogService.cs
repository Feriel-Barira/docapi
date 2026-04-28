using DocApi.DTOs;

namespace DocApi.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityType, string? entityId, string? details = null);
        Task<PagedResultDto<AuditLogDto>> GetFilteredAsync(AuditLogFilterDto filter);
        Task<IEnumerable<AuditLogDto>> GetRecentAsync(int count = 20);
    }
}

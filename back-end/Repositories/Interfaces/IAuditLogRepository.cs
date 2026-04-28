using DocApi.Domain.Entities;
using DocApi.DTOs;

namespace DocApi.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task LogAsync(AuditLog log);
        Task<PagedResultDto<AuditLogDto>> GetFilteredAsync(AuditLogFilterDto filter);
        Task<IEnumerable<AuditLogDto>> GetRecentAsync(int count = 20);
    }
}
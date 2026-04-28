using DocApi.DTOs;

namespace DocApi.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(Guid organisationId);
    }
}

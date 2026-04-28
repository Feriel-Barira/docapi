using DocApi.DTOs;

namespace DocApi.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<StatistiquesGeneralesDto> GetStatistiquesAsync(Guid organisationId);
        Task<RepartitionNcDto> GetRepartitionNcAsync(Guid organisationId);
        Task<List<IndicateurResumDto>> GetIndicateursHorsCibleAsync(Guid organisationId);
    }
}
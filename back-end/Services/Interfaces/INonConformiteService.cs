// Services/Interfaces/INonConformiteService.cs
using DocApi.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Services.Interfaces
{
    public interface INonConformiteService
    {
        Task<NonConformiteDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<NonConformiteDto>> GetAllAsync(Guid organisationId, string userRole, int userId);
        Task<IEnumerable<NonConformiteDto>> GetByProcessusAsync(Guid processusId);
        Task<IEnumerable<NonConformiteDto>> GetFilteredAsync(Guid organisationId, NonConformiteFilterDto filter);
        Task<NonConformiteDto> CreateAsync(Guid organisationId, CreateNonConformiteDto dto, int userId);
        Task<NonConformiteDto> UpdateAsync(Guid id, UpdateNonConformiteDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<NonConformiteDto> UpdateStatutAsync(Guid id, string nouveauStatut, int userId, string? commentaire);

        // Analyses
        Task<AnalyseCauseDto> AddAnalyseAsync(Guid nonConformiteId, CreateAnalyseCauseDto dto, int userId);
        Task<AnalyseCauseDto> UpdateAnalyseAsync(Guid analyseId, CreateAnalyseCauseDto dto);

        // Historique
        Task<IEnumerable<HistoriqueNonConformiteDto>> GetHistoriqueAsync(Guid nonConformiteId);
    }
}
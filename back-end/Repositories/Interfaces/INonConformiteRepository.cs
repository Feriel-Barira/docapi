// Repositories/Interfaces/INonConformiteRepository.cs
using DocApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Repositories.Interfaces
{
    public interface INonConformiteRepository
    {
        // NonConformites
        Task<NonConformite?> GetByIdAsync(Guid id);
        Task<IEnumerable<NonConformite>> GetAllAsync(Guid organisationId, string userRole, int userId);
        Task<IEnumerable<NonConformite>> GetByProcessusAsync(Guid processusId);
        Task<IEnumerable<NonConformite>> GetFilteredAsync(Guid organisationId, NonConformiteFilterParams filter);
        Task<string> GenerateReferenceAsync(Guid organisationId, Guid processusId);
        Task<Guid> CreateAsync(NonConformite nonConformite);
        Task<bool> UpdateAsync(NonConformite nonConformite);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> UpdateStatutAsync(Guid id, NonConformiteStatut nouveauStatut, int userId, string? commentaire);

        // Analyses
        Task<AnalyseCauseNonConformite?> GetAnalyseByNonConformiteIdAsync(Guid nonConformiteId);
        Task<Guid> AddAnalyseAsync(AnalyseCauseNonConformite analyse);
        Task<bool> UpdateAnalyseAsync(AnalyseCauseNonConformite analyse);

        // Historique
        Task<IEnumerable<HistoriqueNonConformite>> GetHistoriqueAsync(Guid nonConformiteId);
        Task<Guid> AddHistoriqueAsync(HistoriqueNonConformite historique);
        Task<AnalyseCauseNonConformite?> GetAnalyseByIdAsync(Guid analyseId);
    }

    public class NonConformiteFilterParams
    {
        public Guid? ProcessusId { get; set; }
        public string? Source { get; set; }
        public string? Gravite { get; set; }
        public string? Statut { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
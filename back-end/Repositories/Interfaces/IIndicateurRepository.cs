// Repositories/Interfaces/IIndicateurRepository.cs
using DocApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Repositories.Interfaces
{
    public interface IIndicateurRepository
    {
        // Indicateurs
        Task<Indicateur?> GetByIdAsync(Guid id);
        Task<IEnumerable<Indicateur>> GetAllAsync(Guid organisationId);
        Task<IEnumerable<Indicateur>> GetByProcessusAsync(Guid processusId);
        Task<IEnumerable<Indicateur>> GetFilteredAsync(Guid organisationId, string? searchTerm, bool? actif);
        Task<Indicateur?> GetByCodeAsync(string code, Guid organisationId);
        Task<Guid> CreateAsync(Indicateur indicateur);
        Task<bool> UpdateAsync(Indicateur indicateur);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null);

        // Valeurs
        Task<IEnumerable<IndicateurValeur>> GetValeursAsync(Guid indicateurId, int limit = 12);
        Task<IndicateurValeur?> GetDerniereValeurAsync(Guid indicateurId);
        Task<IndicateurValeur?> GetValeurByPeriodeAsync(Guid indicateurId, string periode);
        Task<Guid> AddValeurAsync(IndicateurValeur valeur);
        Task<bool> UpdateValeurAsync(IndicateurValeur valeur);
        Task<bool> DeleteValeurAsync(Guid id);
        
        // Dashboard
        Task<int> GetIndicateursCountAsync(Guid organisationId, bool? actif = null);
        Task<decimal?> GetTendanceAsync(Guid indicateurId, int mois = 3);
    }
}
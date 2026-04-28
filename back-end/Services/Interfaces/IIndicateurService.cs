// Services/Interfaces/IIndicateurService.cs
using DocApi.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Services.Interfaces
{
    public interface IIndicateurService
    {
        // Indicateurs
        Task<IndicateurDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<IndicateurDto>> GetAllAsync(Guid organisationId);
        Task<IEnumerable<IndicateurDto>> GetByProcessusAsync(Guid processusId);
        Task<IEnumerable<IndicateurDto>> GetFilteredAsync(Guid organisationId, IndicateurFilterDto filter);
        Task<IndicateurDto> CreateAsync(Guid organisationId, Guid processusId, CreateIndicateurDto dto);
        Task<IndicateurDto> UpdateAsync(Guid id, UpdateIndicateurDto dto);
        Task<bool> DeleteAsync(Guid id);

        // Valeurs
        Task<IEnumerable<IndicateurValeurDto>> GetValeursAsync(Guid indicateurId, int limit = 12);
        Task<IndicateurValeurDto> AddValeurAsync(Guid indicateurId, CreateIndicateurValeurDto dto, int userId);
        Task<IndicateurValeurDto> UpdateValeurAsync(Guid valeurId, CreateIndicateurValeurDto dto);
        Task<bool> DeleteValeurAsync(Guid valeurId);
    }
}
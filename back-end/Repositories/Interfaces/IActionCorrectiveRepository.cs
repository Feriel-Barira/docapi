// Repositories/Interfaces/IActionCorrectiveRepository.cs
using DocApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Repositories.Interfaces
{
    public interface IActionCorrectiveRepository
    {
        Task<ActionCorrective?> GetByIdAsync(Guid id);
        Task<IEnumerable<ActionCorrective>> GetByNonConformiteAsync(Guid nonConformiteId);
        Task<Guid> CreateAsync(ActionCorrective action);
        Task<bool> UpdateAsync(ActionCorrective action);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<ActionCorrective>> GetEcheanceProcheAsync(Guid organisationId, int joursAlerte = 7);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocApi.Domain; // suppose que vous avez une classe Enregistrement

namespace DocApi.Repositories.Interfaces
{
    public interface IEnregistrementRepository
    {
        Task<IEnumerable<Enregistrement>> GetAllAsync(Guid? processusId, string userRole, int userId);
        Task<Enregistrement?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(Enregistrement enreg);
        Task DeleteAsync(Guid id);
        Task<string?> GetLastReferenceAsync(Guid organisationId, int year);
    }
}
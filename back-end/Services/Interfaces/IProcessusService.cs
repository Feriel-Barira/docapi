using DocApi.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Services.Interfaces
{
    public interface IProcessusService
    {
        // Ajout du paramètre userRole pour la gestion Admin/User
        Task<IEnumerable<ProcessusDto>> GetAllAsync(Guid organisationId, string userRole, int userId);

        Task<ProcessusDto?> GetByIdAsync(Guid id);

        // Ajout du paramètre userRole pour le filtrage
        Task<IEnumerable<ProcessusDto>> GetFilteredAsync(Guid organisationId, string userRole, int userId, ProcessusFilterDto filter);

        Task<ProcessusDto> CreateAsync(Guid organisationId, CreateProcessusDto createDto);
        Task<ProcessusDto> UpdateAsync(Guid id, UpdateProcessusDto updateDto);
        Task<bool> DeleteAsync(Guid id);

        Task<IEnumerable<ProcessusActeurDto>> GetActeursAsync(Guid processusId);
        Task<ProcessusActeurDto> AddActeurAsync(Guid processusId, CreateProcessusActeurDto acteurDto);
        Task<bool> RemoveActeurAsync(Guid acteurId);

        Task<int> GetProceduresCountAsync(Guid processusId);
        Task<int> GetDocumentsCountAsync(Guid processusId);
        Task<bool> CodeExistsAsync(string code, string organisationId, Guid? excludeId = null);
    }
}
// Repositories/Interfaces/IProcedureRepository.cs
using DocApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Repositories.Interfaces
{
    public interface IProcedureRepository
    {
        // Procédures
        Task<Procedure?> GetByIdAsync(Guid id);
        Task<IEnumerable<Procedure>> GetAllAsync(Guid organisationId, string userRole, int userId);
        Task<IEnumerable<Procedure>> GetByProcessusAsync(Guid processusId, string userRole, int userId);
        Task<IEnumerable<Procedure>> GetFilteredAsync(Guid organisationId, string userRole, int userId, ProcedureFilterParams filter);
        Task<Procedure?> GetByCodeAsync(string code, Guid organisationId);
        Task<Guid> CreateAsync(Procedure procedure);
        Task<bool> UpdateAsync(Procedure procedure);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null);

        // Instructions
        Task<IEnumerable<Instruction>> GetInstructionsAsync(Guid procedureId);
        Task<Instruction?> GetInstructionByIdAsync(Guid id);
        Task<Guid> AddInstructionAsync(Instruction instruction);
        Task<bool> UpdateInstructionAsync(Instruction instruction);
        Task<bool> DeleteInstructionAsync(Guid id);

        // Compteurs
        Task<int> GetInstructionsCountAsync(Guid procedureId);
    }

    public class ProcedureFilterParams
    {
        public Guid? ProcessusId { get; set; }
        public string? SearchTerm { get; set; }
        public string? Statut { get; set; }
        public int? ResponsableId { get; set; }
    }
}
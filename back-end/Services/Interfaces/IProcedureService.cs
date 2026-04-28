// Services/Interfaces/IProcedureService.cs
using DocApi.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Services.Interfaces
{
    public interface IProcedureService
    {
        // Procédures
        Task<ProcedureDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProcedureDto>> GetAllAsync(Guid organisationId, string userRole, int userId);
        Task<IEnumerable<ProcedureDto>> GetByProcessusAsync(Guid processusId, string userRole, int userId);
        Task<IEnumerable<ProcedureDto>> GetFilteredAsync(Guid organisationId, string userRole, int userId, ProcedureFilterDto filter);
        Task<ProcedureDto> CreateAsync(Guid organisationId, Guid processusId, CreateProcedureDto dto);
        Task<ProcedureDto> UpdateAsync(Guid id, UpdateProcedureDto dto);
        Task<bool> DeleteAsync(Guid id);

        // Instructions
        Task<IEnumerable<InstructionDto>> GetInstructionsAsync(Guid procedureId);
        Task<InstructionDto> AddInstructionAsync(Guid procedureId, CreateInstructionDto dto);
        Task<InstructionDto> UpdateInstructionAsync(Guid instructionId, UpdateInstructionDto dto);
        Task<bool> DeleteInstructionAsync(Guid instructionId);
        Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null);
    }
}
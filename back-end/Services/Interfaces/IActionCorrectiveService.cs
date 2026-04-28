// Services/Interfaces/IActionCorrectiveService.cs
using DocApi.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Services.Interfaces
{
    public interface IActionCorrectiveService
    {
        Task<ActionCorrectiveDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ActionCorrectiveDto>> GetByNonConformiteAsync(Guid nonConformiteId);
        Task<ActionCorrectiveDto> CreateAsync(Guid nonConformiteId, CreateActionCorrectiveDto dto);
        Task<ActionCorrectiveDto> UpdateAsync(Guid id, UpdateActionCorrectiveDto dto);
        Task<ActionCorrectiveDto> RealiserAsync(Guid id, string commentaire, int userId);
        Task<ActionCorrectiveDto> VerifierAsync(Guid id, bool efficace, string? commentaire, int userId);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<ActionCorrectiveDto>> GetEcheanceProcheAsync(Guid organisationId, int joursAlerte = 7);
        Task AttacherPreuveAsync(Guid id, Guid enregistrementId);
        Task DetacherPreuveAsync(Guid id);
    }
}
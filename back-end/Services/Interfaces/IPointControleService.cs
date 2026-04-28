using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocApi.DTOs;

namespace DocApi.Services.Interfaces

{
    public interface IPointControleService
    {
        Task<IEnumerable<PointControleResponseDto>> GetByOrganisationIdAsync(Guid organisationId);
        Task<PointControleResponseDto?> GetByIdAsync(Guid id);
        Task<PointControleResponseDto> CreateAsync(Guid organisationId, PointControleCreateDto dto);
        Task<PointControleResponseDto?> UpdateAsync(Guid id, PointControleCreateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<EvaluationResponseDto>> GetEvaluationsAsync(Guid pointControleId);
        Task<EvaluationResponseDto> AddEvaluationAsync(Guid pointControleId, EvaluationCreateDto dto);
    }
}
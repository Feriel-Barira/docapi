using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocApi.Domain;

namespace DocApi.Repositories.Interfaces
{
    public interface IPointControleRepository
    {
        Task<IEnumerable<PointControle>> GetByOrganisationIdAsync(Guid organisationId);
        Task<PointControle?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(PointControle pointControle);
        Task<bool> UpdateAsync(PointControle pointControle);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<EvaluationPointControle>> GetEvaluationsByPointControleIdAsync(Guid pointControleId);
        Task<Guid> AddEvaluationAsync(EvaluationPointControle evaluation);
    }
}
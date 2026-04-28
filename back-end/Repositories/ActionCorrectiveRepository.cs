using Dapper;
using DocApi.Domain.Entities;
using DocApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DocApi.Repositories
{
    public class ActionCorrectiveRepository : IActionCorrectiveRepository
    {
        private readonly IDbConnection _connection;

        public ActionCorrectiveRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ActionCorrective?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT *, preuveEnregistrementId AS PreuveEnregistrementId FROM ActionsCorrectives WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<ActionCorrective>(sql, new { Id = id.ToString() });
        }

        public async Task<IEnumerable<ActionCorrective>> GetByNonConformiteAsync(Guid nonConformiteId)
        {
            const string sql = "SELECT *, preuveEnregistrementId AS PreuveEnregistrementId FROM ActionsCorrectives WHERE NonConformiteId = @NcId ORDER BY DateCreation DESC";
            return await _connection.QueryAsync<ActionCorrective>(sql, new { NcId = nonConformiteId.ToString() });
        }

        public async Task<Guid> CreateAsync(ActionCorrective action)
        {
            const string sql = @"
                INSERT INTO ActionsCorrectives (Id, NonConformiteId, Type, Description, ResponsableId, 
                                               DateEcheance, Statut, DateCreation)
                VALUES (@Id, @NcId, @Type, @Description, @RespId, @DateEcheance, @Statut, NOW())";
            action.Id = Guid.NewGuid();
            action.DateCreation = DateTime.UtcNow;
            action.Statut = ActionCorrectiveStatut.PLANIFIEE;
            await _connection.ExecuteAsync(sql, new
            {
                Id = action.Id.ToString(),
                NcId = action.NonConformiteId.ToString(),
                Type = action.Type.ToString(),
                action.Description,
                RespId = action.ResponsableId,
                action.DateEcheance,
                Statut = action.Statut.ToString()
            });
            return action.Id;
        }

        public async Task<bool> UpdateAsync(ActionCorrective action)
        {
            const string sql = @"
                UPDATE ActionsCorrectives 
                SET Description = @Description, ResponsableId = @RespId, DateEcheance = @DateEcheance,
                    Statut = @Statut, DateRealisation = @DateRealisation, PreuveEnregistrementId = @PreuveEnregistrementId,
                    DateModification = NOW()
                WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = action.Id.ToString(),
                action.Description,
                RespId = action.ResponsableId,
                action.DateEcheance,
                Statut = action.Statut.ToString(),
                action.DateRealisation,
                PreuveEnregistrementId = action.PreuveEnregistrementId?.ToString()
            });
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM ActionsCorrectives WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            const string sql = "SELECT COUNT(1) FROM ActionsCorrectives WHERE Id = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id.ToString() });
            return count > 0;
        }

        public async Task<IEnumerable<ActionCorrective>> GetEcheanceProcheAsync(Guid organisationId, int joursAlerte = 7)
        {
            var sql = @"
                SELECT ac.* FROM ActionsCorrectives ac
                JOIN NonConformites nc ON ac.NonConformiteId = nc.Id
                WHERE nc.OrganisationId = @OrgId
                  AND ac.Statut != 'VERIFIEE'
                  AND ac.DateEcheance BETWEEN NOW() AND DATE_ADD(NOW(), INTERVAL @JoursAlerte DAY)
                ORDER BY ac.DateEcheance ASC";
            return await _connection.QueryAsync<ActionCorrective>(sql, new
            {
                OrgId = organisationId.ToString(),
                JoursAlerte = joursAlerte
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DocApi.Domain;
using DocApi.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace DocApi.Repositories
{
    public class EnregistrementRepository : IEnregistrementRepository
    {
        private readonly IDbConnection _connection;

        public EnregistrementRepository(IDbConnection connection)
        {
            _connection = connection;
        }

       public async Task<IEnumerable<Enregistrement>> GetAllAsync(Guid? processusId, string userRole, int userId)
{
    string sql;
    var parameters = new DynamicParameters();
    parameters.Add("processusId", processusId?.ToString());

    if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
    {
        sql = @"
            SELECT e.*, p.Nom AS ProcessusNom, p.Code AS ProcessusCode, u.Nom AS CreeParNom
            FROM enregistrement e
            LEFT JOIN processus p ON e.processusid = p.id
            LEFT JOIN users u ON e.creeparid = u.id
            WHERE (@processusId IS NULL OR e.processusid = @processusId)
            ORDER BY e.dateenregistrement DESC";
    }
    else
    {
        sql = @"
            SELECT e.*, p.Nom AS ProcessusNom, p.Code AS ProcessusCode, u.Nom AS CreeParNom
            FROM enregistrement e
            LEFT JOIN processus p ON e.processusid = p.id
            LEFT JOIN users u ON e.creeparid = u.id
            WHERE (@processusId IS NULL OR e.processusid = @processusId)
              AND (
                  e.CreeParId = @UserId
                  OR EXISTS (
                      SELECT 1 FROM processus pr
                      LEFT JOIN processusacteurs pa ON pa.processusid = pr.id AND pa.utilisateurid = @UserId
                      WHERE pr.id = e.processusid
                        AND (pr.piloteid = @UserId OR pa.utilisateurid IS NOT NULL)
                  )
              )
            ORDER BY e.dateenregistrement DESC";
        parameters.Add("UserId", userId);
    }

    return await _connection.QueryAsync<Enregistrement>(sql, parameters);
}

        public async Task<Enregistrement?> GetByIdAsync(Guid id)
        {
            var sql = "SELECT * FROM enregistrement WHERE id = @id";
            return await _connection.QueryFirstOrDefaultAsync<Enregistrement>(sql, new { id });
        }

        public async Task<Guid> CreateAsync(Enregistrement enreg)
        {
            var sql = @"
                INSERT INTO enregistrement (id, organisationid, processusid, typeenregistrement, reference, description, fichierpath, dateenregistrement, creeparid)
                VALUES (@Id, @OrganisationId, @ProcessusId, @TypeEnregistrement, @Reference, @Description, @FichierPath, @DateEnregistrement, @CreeParId)";

            enreg.Id = Guid.NewGuid();
            await _connection.ExecuteAsync(sql, enreg);
            return enreg.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            var sql = "DELETE FROM enregistrement WHERE id = @id";
            await _connection.ExecuteAsync(sql, new { id });
        }
        public async Task<string?> GetLastReferenceAsync(Guid organisationId, int year)
        {
            var sql = @"
        SELECT reference 
        FROM enregistrement 
        WHERE organisationid = @organisationId 
          AND reference LIKE @prefix
        ORDER BY reference DESC 
        LIMIT 1";
            var prefix = $"ENREG-{year}-%";
            return await _connection.QueryFirstOrDefaultAsync<string>(sql, new { organisationId, prefix });
        }
    }
}
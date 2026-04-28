// Repositories/ProcedureRepository.cs
using Dapper;
using DocApi.Domain.Entities;
using DocApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DocApi.Repositories
{
    public class ProcedureRepository : IProcedureRepository
    {
        private readonly IDbConnection _connection;

        public ProcedureRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        // ==================== PROCÉDURES ====================

        public async Task<Procedure?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM Procedures WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<Procedure>(sql, new { Id = id.ToString() });
        }

        public async Task<IEnumerable<Procedure>> GetAllAsync(Guid organisationId, string userRole, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            string sql;

            if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
            {
                sql = @"
            SELECT p.Id, p.OrganisationId, p.ProcessusId, p.Code, p.Titre, p.Objectif,
                   p.DomaineApplication, p.Description, p.ResponsableId, p.Statut,
                   p.DateCreation, p.DateModification,
                   COALESCE(pr.Code, '') AS ProcessusCode,
                   COALESCE(pr.Nom, '')  AS ProcessusNom
            FROM Procedures p
            LEFT JOIN Processus pr ON p.ProcessusId = pr.Id
            WHERE p.OrganisationId = @OrgId
            ORDER BY p.DateCreation DESC";
            }
            else
            {
                sql = @"
            SELECT DISTINCT p.Id, p.OrganisationId, p.ProcessusId, p.Code, p.Titre, p.Objectif,
                   p.DomaineApplication, p.Description, p.ResponsableId, p.Statut,
                   p.DateCreation, p.DateModification,
                   COALESCE(pr.Code, '') AS ProcessusCode,
                   COALESCE(pr.Nom, '')  AS ProcessusNom
            FROM Procedures p
            LEFT JOIN Processus pr ON p.ProcessusId = pr.Id
            WHERE p.OrganisationId = @OrgId
              AND (
                  p.ResponsableId = @UserId
                  OR EXISTS (
                      SELECT 1 FROM Processus pr2
                      LEFT JOIN ProcessusActeurs pa ON pa.ProcessusId = pr2.Id AND pa.UtilisateurId = @UserId
                      WHERE pr2.Id = p.ProcessusId
                        AND (pr2.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL)
                  )
              )
            ORDER BY p.DateCreation DESC";
                parameters.Add("UserId", userId);
            }

            var results = await _connection.QueryAsync<Procedure>(sql, parameters);
            foreach (var r in results)
                Console.WriteLine($"=== PROC: {r.Code} | ProcessusCode: '{r.ProcessusCode}' | ProcessusNom: '{r.ProcessusNom}'");

            return results;
        }

        public async Task<IEnumerable<Procedure>> GetByProcessusAsync(Guid processusId, string userRole, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("ProcId", processusId.ToString());

            string sql;

            if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
            {
                sql = @"SELECT p.* 
                FROM Procedures p
                WHERE p.ProcessusId = @ProcId
                ORDER BY p.DateCreation DESC";
            }
            else // AUDITEUR ou UTILISATEUR
            {
                sql = @"SELECT DISTINCT p.*
                FROM Procedures p
                INNER JOIN Processus pr ON p.ProcessusId = pr.Id
                LEFT JOIN ProcessusActeurs pa ON pa.ProcessusId = pr.Id AND pa.UtilisateurId = @UserId
                WHERE p.ProcessusId = @ProcId
                  AND (pr.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL OR p.ResponsableId = @UserId)
                ORDER BY p.DateCreation DESC";
                parameters.Add("UserId", userId);
            }

            return await _connection.QueryAsync<Procedure>(sql, parameters);
        }

        public async Task<IEnumerable<Procedure>> GetFilteredAsync(Guid organisationId, string userRole, int userId, ProcedureFilterParams filter)
        {
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            string sql;

            if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
            {
                sql = "SELECT p.* FROM Procedures p WHERE p.OrganisationId = @OrgId";
            }
            else // AUDITEUR ou UTILISATEUR
            {
                sql = @"SELECT DISTINCT p.*
                FROM Procedures p
                INNER JOIN Processus pr ON p.ProcessusId = pr.Id
                LEFT JOIN ProcessusActeurs pa ON pa.ProcessusId = pr.Id AND pa.UtilisateurId = @UserId
                WHERE p.OrganisationId = @OrgId
                  AND (pr.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL OR p.ResponsableId = @UserId)";
                parameters.Add("UserId", userId);
            }

            // === Le reste est IDENTIQUE (filtres, pagination, etc.) ===
            if (filter.ProcessusId.HasValue)
            {
                sql += " AND p.ProcessusId = @ProcessusId";
                parameters.Add("ProcessusId", filter.ProcessusId.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                sql += " AND (p.Titre LIKE @SearchTerm OR p.Code LIKE @SearchTerm OR p.Description LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{filter.SearchTerm}%");
            }

            if (!string.IsNullOrWhiteSpace(filter.Statut))
            {
                sql += " AND p.Statut = @Statut";
                parameters.Add("Statut", filter.Statut);
            }

            if (filter.ResponsableId.HasValue)
            {
                sql += " AND p.ResponsableId = @ResponsableId";
                parameters.Add("ResponsableId", filter.ResponsableId.Value);
            }

            sql += " ORDER BY p.DateCreation DESC";

            return await _connection.QueryAsync<Procedure>(sql, parameters);
        }

        public async Task<Procedure?> GetByCodeAsync(string code, Guid organisationId)
        {
            const string sql = "SELECT * FROM Procedures WHERE Code = @Code AND OrganisationId = @OrgId";
            return await _connection.QueryFirstOrDefaultAsync<Procedure>(sql, new { Code = code, OrgId = organisationId.ToString() });
        }

        public async Task<Guid> CreateAsync(Procedure procedure)
        {
            const string sql = @"
                INSERT INTO Procedures (Id, OrganisationId, ProcessusId, Code, Titre, Objectif, 
                                       DomaineApplication, Description, ResponsableId, Statut, DateCreation)
                VALUES (@Id, @OrgId, @ProcId, @Code, @Titre, @Objectif, @Domaine, @Description, 
                        @RespId, @Statut, NOW())";

            procedure.Id = Guid.NewGuid();
            procedure.DateCreation = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = procedure.Id.ToString(),
                OrgId = procedure.OrganisationId.ToString(),
                ProcId = procedure.ProcessusId.ToString(),
                procedure.Code,
                procedure.Titre,
                procedure.Objectif,
                Domaine = procedure.DomaineApplication,
                procedure.Description,
                RespId = procedure.ResponsableId,
                Statut = procedure.Statut.ToString()
            });

            return procedure.Id;
        }

        public async Task<bool> UpdateAsync(Procedure procedure)
        {
            const string sql = @"
                UPDATE Procedures 
                SET Code = @Code, Titre = @Titre, Objectif = @Objectif,
                    DomaineApplication = @Domaine, Description = @Description,
                    ResponsableId = @RespId, Statut = @Statut,
                    DateModification = NOW()
                WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = procedure.Id.ToString(),
                procedure.Code,
                procedure.Titre,
                procedure.Objectif,
                Domaine = procedure.DomaineApplication,
                procedure.Description,
                RespId = procedure.ResponsableId,
                Statut = procedure.Statut.ToString()
            });

            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM Procedures WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            const string sql = "SELECT COUNT(1) FROM Procedures WHERE Id = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id.ToString() });
            return count > 0;
        }

        public async Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null)
        {
            var sql = "SELECT COUNT(1) FROM Procedures WHERE Code = @Code AND OrganisationId = @OrgId";
            var parameters = new DynamicParameters();
            parameters.Add("Code", code);
            parameters.Add("OrgId", organisationId.ToString());

            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                parameters.Add("ExcludeId", excludeId.Value.ToString());
            }

            var count = await _connection.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }

        // ==================== INSTRUCTIONS ====================

        public async Task<IEnumerable<Instruction>> GetInstructionsAsync(Guid procedureId)
        {
            const string sql = "SELECT * FROM Instructions WHERE ProcedureId = @ProcId ORDER BY Ordre ASC";
            return await _connection.QueryAsync<Instruction>(sql, new { ProcId = procedureId.ToString() });
        }

        public async Task<Instruction?> GetInstructionByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM Instructions WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<Instruction>(sql, new { Id = id.ToString() });
        }

        public async Task<Guid> AddInstructionAsync(Instruction instruction)
        {
            const string sql = @"
                INSERT INTO Instructions (Id, OrganisationId, ProcedureId, Code, Titre, Description, Ordre, Statut, DateCreation)
                VALUES (@Id, @OrgId, @ProcId, @Code, @Titre, @Description, @Ordre, @Statut, NOW())";

            instruction.Id = Guid.NewGuid();
            instruction.DateCreation = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = instruction.Id.ToString(),
                OrgId = instruction.OrganisationId.ToString(),
                ProcId = instruction.ProcedureId.ToString(),
                instruction.Code,
                instruction.Titre,
                instruction.Description,
                instruction.Ordre,
                Statut = instruction.Statut.ToString()
            });

            return instruction.Id;
        }

        public async Task<bool> UpdateInstructionAsync(Instruction instruction)
        {
            const string sql = @"
                UPDATE Instructions 
                SET Code = @Code, Titre = @Titre, Description = @Description,
                    Ordre = @Ordre, Statut = @Statut
                WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = instruction.Id.ToString(),
                instruction.Code,
                instruction.Titre,
                instruction.Description,
                instruction.Ordre,
                Statut = instruction.Statut.ToString()
            });

            return affected > 0;
        }

        public async Task<bool> DeleteInstructionAsync(Guid id)
        {
            const string sql = "DELETE FROM Instructions WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<int> GetInstructionsCountAsync(Guid procedureId)
        {
            const string sql = "SELECT COUNT(1) FROM Instructions WHERE ProcedureId = @ProcId";
            return await _connection.ExecuteScalarAsync<int>(sql, new { ProcId = procedureId.ToString() });
        }
        private class ProcessusInfo
        {
            public string? Code { get; set; }
            public string? Nom { get; set; }
        }
    }
}
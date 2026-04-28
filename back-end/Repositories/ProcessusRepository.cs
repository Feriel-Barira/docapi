using Dapper;
using DocApi.Domain.Entities;
using DocApi.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace DocApi.Repositories
{
    public class ProcessusRepository : IProcessusRepository
    {
        private readonly IDbConnection _connection;

        public ProcessusRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Processus>> GetAllAsync(
     Guid organisationId, string userRole, int userId)
        {
            string sql;

            if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
            {
                // Voient TOUS les processus de leur organisation
                sql = @"SELECT * FROM Processus 
                WHERE OrganisationId = @OrganisationId 
                ORDER BY DateCreation DESC";
                return await _connection.QueryAsync<Processus>(sql,
                    new { OrganisationId = organisationId.ToString() });
            }
            else if (userRole == "AUDITEUR" || userRole == "UTILISATEUR")
            {
                // Voient seulement les processus où ils sont pilote OU acteur
                sql = @"SELECT DISTINCT p.* FROM Processus p
                LEFT JOIN ProcessusActeurs pa 
                    ON pa.ProcessusId = p.Id AND pa.UtilisateurId = @UserId
                WHERE p.OrganisationId = @OrganisationId
                  AND (p.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL)
                ORDER BY p.DateCreation DESC";
                return await _connection.QueryAsync<Processus>(sql, new
                {
                    OrganisationId = organisationId.ToString(),
                    UserId = userId
                });
            }

            return Enumerable.Empty<Processus>();
        }

        public async Task<Processus?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM Processus WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<Processus>(sql, new { Id = id.ToString() });
        }

        public async Task<IEnumerable<Processus>> GetFilteredAsync(
    Guid organisationId, string userRole, int userId,
    string? searchTerm, string? type, string? statut, int? piloteId)
        {
            string sql;
            var parameters = new DynamicParameters();
            parameters.Add("OrganisationId", organisationId.ToString());

            if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
            {
                sql = "SELECT * FROM Processus WHERE OrganisationId = @OrganisationId";
            }
            else
            {
                // AUDITEUR / UTILISATEUR → seulement pilote ou acteur
                sql = @"SELECT DISTINCT p.* FROM Processus p
                LEFT JOIN ProcessusActeurs pa 
                    ON pa.ProcessusId = p.Id AND pa.UtilisateurId = @UserId
                WHERE p.OrganisationId = @OrganisationId
                  AND (p.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL)";
                parameters.Add("UserId", userId);
            }

            // Filtres communs
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (Nom LIKE @SearchTerm OR Code LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                sql += " AND Type = @Type";
                parameters.Add("Type", type);
            }
            if (!string.IsNullOrWhiteSpace(statut))
            {
                sql += " AND Statut = @Statut";
                parameters.Add("Statut", statut);
            }

            sql += " ORDER BY DateCreation DESC";

            return await _connection.QueryAsync<Processus>(sql, parameters);
        }

        public async Task<Processus?> GetByCodeAsync(string code, Guid organisationId)
        {
            const string sql = "SELECT * FROM Processus WHERE Code = @Code AND OrganisationId = @OrganisationId";
            return await _connection.QueryFirstOrDefaultAsync<Processus>(sql, new { Code = code, OrganisationId = organisationId.ToString() });
        }

        public async Task<Guid> CreateAsync(Processus processus)
        {
            const string sql = @"
                INSERT INTO Processus (Id, OrganisationId, Code, Nom, Description, Type, 
                                       Finalites, Perimetres, Fournisseurs, Clients, 
                                       DonneesEntree, DonneesSortie, Objectifs, 
                                       PiloteId, Statut, DateCreation)
                VALUES (@Id, @OrganisationId, @Code, @Nom, @Description, @Type,
                        @Finalites, @Perimetres, @Fournisseurs, @Clients,
                        @DonneesEntree, @DonneesSortie, @Objectifs,
                        @PiloteId, @Statut, @DateCreation)";

            processus.Id = Guid.NewGuid();
            processus.DateCreation = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = processus.Id.ToString(),
                OrganisationId = processus.OrganisationId.ToString(),
                processus.Code,
                processus.Nom,
                processus.Description,
                Type = processus.Type.ToString(),
                processus.Finalites,
                processus.Perimetres,
                processus.Fournisseurs,
                processus.Clients,
                processus.DonneesEntree,
                processus.DonneesSortie,
                processus.Objectifs,
                processus.PiloteId,
                Statut = processus.Statut.ToString(),
                processus.DateCreation
            });

            return processus.Id;
        }

        public async Task<bool> UpdateAsync(Processus processus)
        {
            const string sql = @"
                UPDATE Processus 
                SET Code = @Code, 
                    Nom = @Nom, 
                    Description = @Description,
                    Type = @Type, 
                    Finalites = @Finalites, 
                    Perimetres = @Perimetres,
                    Fournisseurs = @Fournisseurs, 
                    Clients = @Clients,
                    DonneesEntree = @DonneesEntree, 
                    DonneesSortie = @DonneesSortie,
                    Objectifs = @Objectifs, 
                    PiloteId = @PiloteId, 
                    Statut = @Statut,
                    DateModification = @DateModification
                WHERE Id = @Id";

            processus.DateModification = DateTime.UtcNow;

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = processus.Id.ToString(),
                processus.Code,
                processus.Nom,
                processus.Description,
                Type = processus.Type.ToString(),
                processus.Finalites,
                processus.Perimetres,
                processus.Fournisseurs,
                processus.Clients,
                processus.DonneesEntree,
                processus.DonneesSortie,
                processus.Objectifs,
                processus.PiloteId,
                Statut = processus.Statut.ToString(),
                processus.DateModification
            });

            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM Processus WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            const string sql = "SELECT COUNT(1) FROM Processus WHERE Id = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id.ToString() });
            return count > 0;
        }

        public async Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null)
        {
            var sql = "SELECT COUNT(1) FROM Processus WHERE Code = @Code AND OrganisationId = @OrganisationId";

            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                var count = await _connection.ExecuteScalarAsync<int>(sql, new { Code = code, OrganisationId = organisationId.ToString(), ExcludeId = excludeId.Value.ToString() });
                return count > 0;
            }

            var result = await _connection.ExecuteScalarAsync<int>(sql, new { Code = code, OrganisationId = organisationId.ToString() });
            return result > 0;
        }

        public async Task<int> GetProceduresCountAsync(Guid processusId)
        {
            const string sql = "SELECT COUNT(1) FROM Procedures WHERE ProcessusId = @ProcessusId AND Statut = 'ACTIF'";
            return await _connection.ExecuteScalarAsync<int>(sql, new { ProcessusId = processusId.ToString() });
        }

        public async Task<int> GetDocumentsCountAsync(Guid processusId)
        {
            const string sql = "SELECT COUNT(1) FROM Document WHERE ProcessusId = @ProcessusId AND Actif = 1";
            return await _connection.ExecuteScalarAsync<int>(sql, new { ProcessusId = processusId.ToString() });
        }

        public async Task<IEnumerable<ProcessusActeur>> GetActeursAsync(Guid processusId)
        {
            const string sql = @"
                SELECT Id, OrganisationId, ProcessusId, UtilisateurId, TypeActeur, DateAffectation
                FROM ProcessusActeurs 
                WHERE ProcessusId = @ProcessusId
                ORDER BY DateAffectation DESC";

            return await _connection.QueryAsync<ProcessusActeur>(sql, new { ProcessusId = processusId.ToString() });
        }

        public async Task<Guid> AddActeurAsync(ProcessusActeur acteur)
        {
            const string sql = @"
                INSERT INTO ProcessusActeurs (Id, OrganisationId, ProcessusId, UtilisateurId, TypeActeur, DateAffectation)
                VALUES (@Id, @OrganisationId, @ProcessusId, @UtilisateurId, @TypeActeur, @DateAffectation)";

            acteur.Id = Guid.NewGuid();
            acteur.DateAffectation = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = acteur.Id.ToString(),
                OrganisationId = acteur.OrganisationId.ToString(),
                ProcessusId = acteur.ProcessusId.ToString(),
                acteur.UtilisateurId,
                TypeActeur = acteur.TypeActeur.ToString(),
                acteur.DateAffectation
            });

            return acteur.Id;
        }

        public async Task<bool> RemoveActeurAsync(Guid acteurId)
        {
            const string sql = "DELETE FROM ProcessusActeurs WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = acteurId.ToString() });
            return affected > 0;
        }
    }
}
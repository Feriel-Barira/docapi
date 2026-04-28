// Repositories/NonConformiteRepository.cs
using Dapper;
using DocApi.Domain.Entities;
using DocApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DocApi.Repositories
{
    public class NonConformiteRepository : INonConformiteRepository
    {
        private readonly IDbConnection _connection;

        public NonConformiteRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        // ==================== NON CONFORMITES ====================

        public async Task<NonConformite?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM NonConformites WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<NonConformite>(sql, new { Id = id.ToString() });
        }

        public async Task<IEnumerable<NonConformite>> GetAllAsync(Guid organisationId, string userRole, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            string sql;

            if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
            {
                sql = "SELECT * FROM NonConformites WHERE OrganisationId = @OrgId ORDER BY DateDetection DESC";
            }
            else
            {
                sql = @"
            SELECT DISTINCT nc.*
            FROM NonConformites nc
            WHERE nc.OrganisationId = @OrgId
              AND (
                  nc.DetecteParId = @UserId
                  OR nc.ResponsableTraitementId = @UserId
                  OR EXISTS (
                      SELECT 1 FROM AnalysesCauses ac
                      WHERE ac.NonConformiteId = nc.Id AND ac.AnalyseParId = @UserId
                  )
                  OR EXISTS (
                      SELECT 1 FROM Processus pr
                      LEFT JOIN ProcessusActeurs pa ON pa.ProcessusId = pr.Id AND pa.UtilisateurId = @UserId
                      WHERE pr.Id = nc.ProcessusId
                        AND (pr.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL)
                  )
              )
            ORDER BY nc.DateDetection DESC";
                parameters.Add("UserId", userId);
            }

            return await _connection.QueryAsync<NonConformite>(sql, parameters);
        }

        public async Task<IEnumerable<NonConformite>> GetByProcessusAsync(Guid processusId)
        {
            const string sql = "SELECT * FROM NonConformites WHERE ProcessusId = @ProcId ORDER BY DateDetection DESC";
            return await _connection.QueryAsync<NonConformite>(sql, new { ProcId = processusId.ToString() });
        }

        public async Task<IEnumerable<NonConformite>> GetFilteredAsync(Guid organisationId, NonConformiteFilterParams filter)
        {
            var sql = "SELECT * FROM NonConformites WHERE OrganisationId = @OrgId";
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            if (filter.ProcessusId.HasValue)
            {
                sql += " AND ProcessusId = @ProcessusId";
                parameters.Add("ProcessusId", filter.ProcessusId.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(filter.Source))
            {
                sql += " AND Source = @Source";
                parameters.Add("Source", filter.Source);
            }

            if (!string.IsNullOrWhiteSpace(filter.Gravite))
            {
                sql += " AND Gravite = @Gravite";
                parameters.Add("Gravite", filter.Gravite);
            }

            if (!string.IsNullOrWhiteSpace(filter.Statut))
            {
                sql += " AND Statut = @Statut";
                parameters.Add("Statut", filter.Statut);
            }

            if (filter.DateDebut.HasValue)
            {
                sql += " AND DateDetection >= @DateDebut";
                parameters.Add("DateDebut", filter.DateDebut.Value);
            }

            if (filter.DateFin.HasValue)
            {
                sql += " AND DateDetection <= @DateFin";
                parameters.Add("DateFin", filter.DateFin.Value);
            }

            sql += " ORDER BY DateDetection DESC";

            return await _connection.QueryAsync<NonConformite>(sql, parameters);
        }

        public async Task<string> GenerateReferenceAsync(Guid organisationId, Guid processusId)
        {
            var annee = DateTime.Now.Year;

            // Récupérer le code du processus
            var processusCode = await _connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Code FROM Processus WHERE Id = @Id", new { Id = processusId.ToString() });

            // Compter les NC pour l'année en cours
            var count = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM NonConformites WHERE OrganisationId = @OrgId AND YEAR(DateDetection) = @Annee",
                new { OrgId = organisationId.ToString(), Annee = annee });

            var numero = (count + 1).ToString("D3");
            return $"{annee}-{processusCode}-{numero}";
        }

        public async Task<Guid> CreateAsync(NonConformite nonConformite)
        {
            Console.WriteLine($"=== REPOSITORY CREATE ===");
            Console.WriteLine($"ResponsableTraitementId: {nonConformite.ResponsableTraitementId}");
            Console.WriteLine($"DetecteParId: {nonConformite.DetecteParId}");
            const string sql = @"
        INSERT INTO NonConformites (Id, OrganisationId, ProcessusId, Reference, DateDetection, 
                                   DetecteParId, Source, Gravite, Statut, Description, 
                                   ResponsableTraitementId,Type, Nature, DateCreation)
        VALUES (@Id, @OrgId, @ProcId, @Reference, @DateDetection, @DetecteParId, 
                @Source, @Gravite, @Statut, @Description, @RespId, @Type, @Nature, NOW())";

            nonConformite.Id = Guid.NewGuid();
            nonConformite.DateCreation = DateTime.UtcNow;
            nonConformite.Statut = NonConformiteStatut.OUVERTE;

            await _connection.ExecuteAsync(sql, new
            {
                Id = nonConformite.Id.ToString(),
                OrgId = nonConformite.OrganisationId.ToString(),
                ProcId = nonConformite.ProcessusId.ToString(),
                nonConformite.Reference,
                nonConformite.DateDetection,
                nonConformite.DetecteParId,
                Source = nonConformite.Source.ToString(),
                Gravite = nonConformite.Gravite.ToString(),
                Statut = nonConformite.Statut.ToString(),
                nonConformite.Description,
                RespId = nonConformite.ResponsableTraitementId,
                Type = nonConformite.Type,      // ← AJOUTER
                Nature = nonConformite.Nature
            });

            return nonConformite.Id;
        }

        public async Task<bool> UpdateAsync(NonConformite nonConformite)
        {
            const string sql = @"
        UPDATE NonConformites 
        SET Description = @Description, 
            Source = @Source,
            Gravite = @Gravite, 
            Statut = @Statut,
            ProcessusId = @ProcessusId,
            ResponsableTraitementId = @RespId,
            DetecteParId = @DetecteParId,
            DateDetection = @DateDetection,
            Type = @Type,
            Nature = @Nature,
            DateModification = NOW()
        WHERE Id = @Id";

            if (nonConformite.Statut == NonConformiteStatut.CLOTUREE && !nonConformite.DateCloture.HasValue)
                nonConformite.DateCloture = DateTime.UtcNow;

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = nonConformite.Id.ToString(),
                nonConformite.Description,
                Source = nonConformite.Source.ToString(),
                Gravite = nonConformite.Gravite.ToString(),
                Statut = nonConformite.Statut.ToString(),
                ProcessusId = nonConformite.ProcessusId.ToString(),
                RespId = nonConformite.ResponsableTraitementId,
                DetecteParId = nonConformite.DetecteParId,
                DateDetection = nonConformite.DateDetection,
                Type = nonConformite.Type,
                Nature = nonConformite.Nature,
                nonConformite.DateCloture
            });

            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM NonConformites WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            const string sql = "SELECT COUNT(1) FROM NonConformites WHERE Id = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id.ToString() });
            return count > 0;
        }

        public async Task<bool> UpdateStatutAsync(Guid id, NonConformiteStatut nouveauStatut, int userId, string? commentaire)
        {
            // Récupérer l'ancien statut
            var ancienStatut = await _connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Statut FROM NonConformites WHERE Id = @Id", new { Id = id.ToString() });

            if (string.IsNullOrEmpty(ancienStatut))
                return false;

            // Mettre à jour le statut
            var sql = "UPDATE NonConformites SET Statut = @Statut, DateModification = NOW() WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = id.ToString(),
                Statut = nouveauStatut.ToString()
            });

            if (affected > 0)
            {
                // Ajouter à l'historique
                await AddHistoriqueAsync(new HistoriqueNonConformite
                {
                    NonConformiteId = id,
                    AncienStatut = Enum.Parse<NonConformiteStatut>(ancienStatut),
                    NouveauStatut = nouveauStatut,
                    DateChangement = DateTime.UtcNow,
                    ChangeParId = userId,
                    Commentaire = commentaire
                });
            }

            return affected > 0;
        }

        // ==================== ANALYSES ====================

        // ==================== ANALYSES ====================

        // À la ligne 220 environ - méthode GetAnalyseByNonConformiteIdAsync
        // ==================== ANALYSES ====================



        // ← AJOUTER cette méthode pour chercher par l'Id de l'analyse
        public async Task<AnalyseCauseNonConformite?> GetAnalyseByIdAsync(Guid analyseId)
        {
            const string sql = "SELECT * FROM AnalysesCauses WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<AnalyseCauseNonConformite>(
                sql, new { Id = analyseId.ToString() });
        }
        public async Task<AnalyseCauseNonConformite?> GetAnalyseByNonConformiteIdAsync(Guid nonConformiteId)
        {
            // ✅ Utiliser "NonConformiteId" (avec e)
            const string sql = "SELECT * FROM AnalysesCauses WHERE NonConformiteId = @NcId";
            return await _connection.QueryFirstOrDefaultAsync<AnalyseCauseNonConformite>(sql, new { NcId = nonConformiteId.ToString() });
        }

        public async Task<Guid> AddAnalyseAsync(AnalyseCauseNonConformite analyse)
        {
            // ✅ Utiliser "NonConformiteId" et "AnalyseParId"
            const string sql = @"
        INSERT INTO AnalysesCauses (Id, NonConformiteId, MethodeAnalyse, Description, DateAnalyse, AnalyseParId)
        VALUES (@Id, @NcId, @Methode, @Description, NOW(), @AnalyseParId)";

            analyse.Id = Guid.NewGuid();
            analyse.DateAnalyse = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = analyse.Id.ToString(),
                NcId = analyse.NonConformiteId.ToString(),
                Methode = analyse.MethodeAnalyse.ToString(),
                analyse.Description,
                AnalyseParId = analyse.AnalyseParId
            });

            return analyse.Id;
        }

        public async Task<bool> UpdateAnalyseAsync(AnalyseCauseNonConformite analyse)
        {
            const string sql = @"
        UPDATE AnalysesCauses 
        SET MethodeAnalyse = @Methode, Description = @Description
        WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = analyse.Id.ToString(),
                Methode = analyse.MethodeAnalyse.ToString(),
                analyse.Description
            });

            return affected > 0;
        }


        // ==================== HISTORIQUE ====================

        public async Task<IEnumerable<HistoriqueNonConformite>> GetHistoriqueAsync(Guid nonConformiteId)
        {
            const string sql = "SELECT * FROM HistoriqueNonConformites WHERE NonConformiteId = @NcId ORDER BY DateChangement DESC";
            return await _connection.QueryAsync<HistoriqueNonConformite>(sql, new { NcId = nonConformiteId.ToString() });
        }

        public async Task<Guid> AddHistoriqueAsync(HistoriqueNonConformite historique)
        {
            const string sql = @"
                INSERT INTO HistoriqueNonConformites (Id, NonConformiteId, AncienStatut, NouveauStatut, DateChangement, ChangeParId, Commentaire)
                VALUES (@Id, @NcId, @Ancien, @Nouveau, NOW(), @ChangeParId, @Commentaire)";

            historique.Id = Guid.NewGuid();
            historique.DateChangement = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = historique.Id.ToString(),
                NcId = historique.NonConformiteId.ToString(),
                Ancien = historique.AncienStatut.ToString(),
                Nouveau = historique.NouveauStatut.ToString(),
                historique.ChangeParId,
                historique.Commentaire
            });

            return historique.Id;
        }
    }
}
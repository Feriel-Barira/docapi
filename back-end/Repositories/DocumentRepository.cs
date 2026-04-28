// Repositories/DocumentRepository.cs
using Dapper;
using DocApi.Domain.Entities;
using DocApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DocApi.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDbConnection _connection;

        public DocumentRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        // ==================== DOCUMENTS ====================

        public async Task<Document?> GetByIdAsync(Guid id)
        {
            const string sql = @"
        SELECT d.*, v.* 
        FROM Documents d
        LEFT JOIN VersionsDocuments v ON d.Id = v.DocumentId
        WHERE d.Id = @Id
        ORDER BY v.DateEtablissement DESC";

            var documentDict = new Dictionary<Guid, Document>();

            await _connection.QueryAsync<Document, VersionDocument, Document>(
                sql,
                (doc, version) =>
                {
                    if (!documentDict.TryGetValue(doc.Id, out var existingDoc))
                    {
                        existingDoc = doc;
                        existingDoc.Versions = new List<VersionDocument>();
                        documentDict.Add(doc.Id, existingDoc);
                    }
                    if (version != null)
                    {
                        existingDoc.Versions.Add(version);
                    }
                    return existingDoc;
                },
                new { Id = id.ToString() },
                splitOn: "Id"
            );

            return documentDict.Values.FirstOrDefault();
        }

        public async Task<IEnumerable<Document>> GetAllAsync(Guid organisationId, string userRole, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            string sql;

            if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
            {
                sql = @"
            SELECT d.*, v.*
            FROM Documents d
            LEFT JOIN VersionsDocuments v ON d.Id = v.DocumentId
            WHERE d.OrganisationId = @OrgId
            ORDER BY d.DateCreation DESC, v.DateEtablissement DESC";
            }
            else
            {
                sql = @"
            SELECT d.*, v.*
            FROM Documents d
            LEFT JOIN VersionsDocuments v ON d.Id = v.DocumentId
            WHERE d.OrganisationId = @OrgId
              AND (
                  d.TypeDocument = 'REFERENCE'
                  OR EXISTS (
                      SELECT 1 FROM VersionsDocuments vv
                      WHERE vv.DocumentId = d.Id
                        AND (vv.EtabliParId = @UserId OR vv.VerifieParId = @UserId OR vv.ValideParId = @UserId)
                  )
                  OR EXISTS (
                      SELECT 1 FROM Processus pr
                      LEFT JOIN ProcessusActeurs pa ON pa.ProcessusId = pr.Id AND pa.UtilisateurId = @UserId
                      WHERE pr.Id = d.ProcessusId
                        AND (pr.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL)
                  )
              )
            ORDER BY d.DateCreation DESC, v.DateEtablissement DESC";
                parameters.Add("UserId", userId);
            }

            var documentDict = new Dictionary<Guid, Document>();

            await _connection.QueryAsync<Document, VersionDocument, Document>(
                sql,
                (doc, version) =>
                {
                    if (!documentDict.TryGetValue(doc.Id, out var existingDoc))
                    {
                        existingDoc = doc;
                        existingDoc.Versions = new List<VersionDocument>();
                        documentDict.Add(doc.Id, existingDoc);
                    }
                    if (version != null)
                        existingDoc.Versions.Add(version);
                    return existingDoc;
                },
                parameters,
                splitOn: "Id"
            );

            return documentDict.Values;
        }

        public async Task<IEnumerable<Document>> GetByProcessusAsync(Guid processusId)
        {
            const string sql = "SELECT * FROM Documents WHERE ProcessusId = @ProcId ORDER BY DateCreation DESC";
            return await _connection.QueryAsync<Document>(sql, new { ProcId = processusId.ToString() });
        }

        public async Task<IEnumerable<Document>> GetFilteredAsync(Guid organisationId, DocumentFilterParams filter)
        {
            var sql = "SELECT * FROM Documents WHERE OrganisationId = @OrgId";
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            if (filter.ProcessusId.HasValue)
            {
                sql += " AND ProcessusId = @ProcessusId";
                parameters.Add("ProcessusId", filter.ProcessusId.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(filter.TypeDocument))
            {
                sql += " AND TypeDocument = @TypeDocument";
                parameters.Add("TypeDocument", filter.TypeDocument);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                sql += " AND (Titre LIKE @SearchTerm OR Code LIKE @SearchTerm OR Description LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{filter.SearchTerm}%");
            }

            if (filter.Actif.HasValue)
            {
                sql += " AND Actif = @Actif";
                parameters.Add("Actif", filter.Actif.Value);
            }

            sql += " ORDER BY DateCreation DESC";

            return await _connection.QueryAsync<Document>(sql, parameters);
        }

        public async Task<Document?> GetByCodeAsync(string code, Guid organisationId)
        {
            const string sql = "SELECT * FROM Documents WHERE Code = @Code AND OrganisationId = @OrgId";
            return await _connection.QueryFirstOrDefaultAsync<Document>(sql, new { Code = code, OrgId = organisationId.ToString() });
        }

        public async Task<Guid> CreateAsync(Document document)
        {
            const string sql = @"
                INSERT INTO Documents (Id, OrganisationId, ProcessusId, Code, Titre, TypeDocument, Description, Actif, DateCreation)
                VALUES (@Id, @OrgId, @ProcId, @Code, @Titre, @Type, @Description, @Actif, NOW())";

            document.Id = Guid.NewGuid();
            document.DateCreation = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = document.Id.ToString(),
                OrgId = document.OrganisationId.ToString(),
                ProcId = document.ProcessusId?.ToString(),
                document.Code,
                document.Titre,
                Type = document.TypeDocument.ToString(),
                document.Description,
                document.Actif
            });

            return document.Id;
        }

        public async Task<bool> UpdateAsync(Document document)
        {
            const string sql = @"
                UPDATE Documents 
                SET Code = @Code, Titre = @Titre, TypeDocument = @Type,
                    Description = @Description, ProcessusId = @ProcId,
                    Actif = @Actif, DateModification = NOW()
                WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = document.Id.ToString(),
                document.Code,
                document.Titre,
                Type = document.TypeDocument.ToString(),
                document.Description,
                ProcId = document.ProcessusId?.ToString(),
                document.Actif
            });

            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM Documents WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            const string sql = "SELECT COUNT(1) FROM Documents WHERE Id = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id.ToString() });
            return count > 0;
        }

        public async Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null)
        {
            var sql = "SELECT COUNT(1) FROM Documents WHERE Code = @Code AND OrganisationId = @OrgId";
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

        // ==================== VERSIONS ====================

        public async Task<IEnumerable<VersionDocument>> GetVersionsAsync(Guid documentId)
        {
            const string sql = "SELECT * FROM VersionsDocuments WHERE DocumentId = @DocId ORDER BY DateEtablissement DESC";
            return await _connection.QueryAsync<VersionDocument>(sql, new { DocId = documentId.ToString() });
        }

        public async Task<VersionDocument?> GetVersionByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM VersionsDocuments WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<VersionDocument>(sql, new { Id = id.ToString() });
        }

        public async Task<VersionDocument?> GetLatestVersionAsync(Guid documentId)
        {
            const string sql = @"
                SELECT * FROM VersionsDocuments 
                WHERE DocumentId = @DocId 
                ORDER BY DateEtablissement DESC 
                LIMIT 1";
            return await _connection.QueryFirstOrDefaultAsync<VersionDocument>(sql, new { DocId = documentId.ToString() });
        }

        // Repositories/DocumentRepository.cs

        public async Task<Guid> AddVersionAsync(VersionDocument version)
        {
            const string sql = @"
        INSERT INTO VersionsDocuments (Id, DocumentId, OrganisationId, NumeroVersion, Statut, 
                                       FichierPath, CommentaireRevision, 
                                       EtabliParId, DateEtablissement,
                                       VerifieParId, DateVerification,
                                       ValideParId, DateValidation,
                                       DateMiseEnVigueur)
        VALUES (@Id, @DocId, @OrgId, @NumVersion, @Statut, 
                @FichierPath, @CommentaireRevision, 
                @EtabliPar, @DateEtablissement,
                @VerifiePar, @DateVerification,
                @ValidePar, @DateValidation,
                @DateMiseEnVigueur)";

            version.Id = Guid.NewGuid();
            if (version.DateEtablissement == default || version.DateEtablissement == DateTime.MinValue)
                version.DateEtablissement = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = version.Id.ToString(),
                DocId = version.DocumentId.ToString(),
                OrgId = version.OrganisationId.ToString(),
                NumVersion = version.NumeroVersion,
                Statut = version.Statut.ToString(),
                FichierPath = version.FichierPath ?? "",
                CommentaireRevision = version.CommentaireRevision ?? "",
                // ✅ EtabliParId est int (non nullable) - direct
                EtabliPar = version.EtabliParId,
                DateEtablissement = version.DateEtablissement,
                // ✅ VerifieParId et ValideParId sont int? - Dapper gère les nullables
                VerifiePar = version.VerifieParId,
                DateVerification = version.DateVerification,
                ValidePar = version.ValideParId,
                DateValidation = version.DateValidation,
                DateMiseEnVigueur = version.DateMiseEnVigueur
            });

            return version.Id;
        }
        public async Task<bool> UpdateVersionAsync(VersionDocument version)
        {
            const string sql = @"
        UPDATE VersionsDocuments 
        SET Statut = @Statut, 
            FichierPath = @FichierPath, 
            CommentaireRevision = @CommentaireRevision,
            VerifieParId = @VerifieParId, 
            DateVerification = @DateVerification,
            ValideParId = @ValideParId, 
            DateValidation = @DateValidation,
            DateMiseEnVigueur = @DateMiseEnVigueur
        WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = version.Id.ToString(),
                Statut = version.Statut.ToString(),
                version.FichierPath,
                CommentaireRevision = version.CommentaireRevision,  // ← Utiliser CommentaireRevision
                version.VerifieParId,
                version.DateVerification,
                version.ValideParId,
                version.DateValidation,
                version.DateMiseEnVigueur
            });

            return affected > 0;
        }

        public async Task<bool> DeleteVersionAsync(Guid id)
        {
            const string sql = "DELETE FROM VersionsDocuments WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<bool> SetVersionAsCurrentAsync(Guid documentId, Guid versionId)
        {
            // Marquer toutes les versions comme non actuelles
            // Note: Ajoutez une colonne 'EstActuelle' si nécessaire
            return true;
        }

        public async Task<int> GetVersionsCountAsync(Guid documentId)
        {
            const string sql = "SELECT COUNT(1) FROM VersionsDocuments WHERE DocumentId = @DocId";
            return await _connection.ExecuteScalarAsync<int>(sql, new { DocId = documentId.ToString() });
        }
    }
}
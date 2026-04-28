// Repositories/IndicateurRepository.cs
using Dapper;
using DocApi.Domain.Entities;
using DocApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DocApi.Repositories
{
    public class IndicateurRepository : IIndicateurRepository
    {
        private readonly IDbConnection _connection;

        public IndicateurRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        // ==================== INDICATEURS ====================

        public async Task<Indicateur?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM Indicateurs WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<Indicateur>(sql, new { Id = id.ToString() });
        }

        public async Task<IEnumerable<Indicateur>> GetAllAsync(Guid organisationId)
        {
            const string sql = "SELECT * FROM Indicateurs WHERE OrganisationId = @OrgId ORDER BY Nom";
            return await _connection.QueryAsync<Indicateur>(sql, new { OrgId = organisationId.ToString() });
        }

        public async Task<IEnumerable<Indicateur>> GetByProcessusAsync(Guid processusId)
        {
            const string sql = "SELECT * FROM Indicateurs WHERE ProcessusId = @ProcId ORDER BY Nom";
            return await _connection.QueryAsync<Indicateur>(sql, new { ProcId = processusId.ToString() });
        }

        public async Task<IEnumerable<Indicateur>> GetFilteredAsync(Guid organisationId, string? searchTerm, bool? actif)
        {
            var sql = "SELECT * FROM Indicateurs WHERE OrganisationId = @OrgId";
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (Nom LIKE @SearchTerm OR Code LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (actif.HasValue)
            {
                sql += " AND Actif = @Actif";
                parameters.Add("Actif", actif.Value);
            }

            sql += " ORDER BY Nom";

            return await _connection.QueryAsync<Indicateur>(sql, parameters);
        }

        public async Task<Indicateur?> GetByCodeAsync(string code, Guid organisationId)
        {
            const string sql = "SELECT * FROM Indicateurs WHERE Code = @Code AND OrganisationId = @OrgId";
            return await _connection.QueryFirstOrDefaultAsync<Indicateur>(sql, new { Code = code, OrgId = organisationId.ToString() });
        }

        public async Task<Guid> CreateAsync(Indicateur indicateur)
        {
            const string sql = @"
        INSERT INTO Indicateurs (Id, OrganisationId, ProcessusId, Code, Nom, Description, 
                                MethodeCalcul, Unite, ValeurCible, SeuilAlerte, 
                                FrequenceMesure, ResponsableId, Actif, DateCreation)
        VALUES (@Id, @OrgId, @ProcId, @Code, @Nom, @Description, @MethodeCalcul, 
                @Unite, @ValeurCible, @SeuilAlerte, @Frequence, @ResponsableId, @Actif, NOW())";

            indicateur.Id = Guid.NewGuid();
            indicateur.DateCreation = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = indicateur.Id.ToString(),
                OrgId = indicateur.OrganisationId.ToString(),
                ProcId = indicateur.ProcessusId.ToString(),
                indicateur.Code,
                indicateur.Nom,
                indicateur.Description,
                indicateur.MethodeCalcul,
                indicateur.Unite,
                indicateur.ValeurCible,
                indicateur.SeuilAlerte,
                Frequence = indicateur.FrequenceMesure.ToString(),
                ResponsableId = indicateur.ResponsableId,   // ← Changé de RespId à ResponsableId
                indicateur.Actif
            });
            return indicateur.Id;
        }

        public async Task<bool> UpdateAsync(Indicateur indicateur)
        {
            const string sql = @"
        UPDATE Indicateurs 
        SET Code = @Code, 
            Nom = @Nom, 
            Description = @Description,
            MethodeCalcul = @MethodeCalcul, 
            Unite = @Unite,
            ValeurCible = @ValeurCible, 
            SeuilAlerte = @SeuilAlerte,
            FrequenceMesure = @Frequence, 
            ResponsableId = @RespId,
            ProcessusId = @ProcessusId,  -- ← AJOUTER CETTE LIGNE
            Actif = @Actif, 
            DateModification = NOW()
        WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = indicateur.Id.ToString(),
                indicateur.Code,
                indicateur.Nom,
                indicateur.Description,
                indicateur.MethodeCalcul,
                indicateur.Unite,
                indicateur.ValeurCible,
                indicateur.SeuilAlerte,
                Frequence = indicateur.FrequenceMesure.ToString(),
                RespId = indicateur.ResponsableId,
                ProcessusId = indicateur.ProcessusId.ToString(),  // ← AJOUTER CETTE LIGNE
                indicateur.Actif
            });

            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM Indicateurs WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            const string sql = "SELECT COUNT(1) FROM Indicateurs WHERE Id = @Id";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id.ToString() });
            return count > 0;
        }

        public async Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null)
        {
            var sql = "SELECT COUNT(1) FROM Indicateurs WHERE Code = @Code AND OrganisationId = @OrgId";
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

        // ==================== VALEURS ====================

        public async Task<IEnumerable<IndicateurValeur>> GetValeursAsync(Guid indicateurId, int limit = 12)
        {
            const string sql = "SELECT * FROM IndicateurValeurs WHERE IndicateurId = @IndId ORDER BY DateMesure DESC LIMIT @Limit";
            return await _connection.QueryAsync<IndicateurValeur>(sql, new { IndId = indicateurId.ToString(), Limit = limit });
        }

        public async Task<IndicateurValeur?> GetDerniereValeurAsync(Guid indicateurId)
        {
            const string sql = "SELECT * FROM IndicateurValeurs WHERE IndicateurId = @IndId ORDER BY DateMesure DESC LIMIT 1";
            return await _connection.QueryFirstOrDefaultAsync<IndicateurValeur>(sql, new { IndId = indicateurId.ToString() });
        }

        public async Task<IndicateurValeur?> GetValeurByPeriodeAsync(Guid indicateurId, string periode)
        {
            const string sql = "SELECT * FROM IndicateurValeurs WHERE IndicateurId = @IndId AND Periode = @Periode";
            return await _connection.QueryFirstOrDefaultAsync<IndicateurValeur>(sql, new { IndId = indicateurId.ToString(), Periode = periode });
        }

        public async Task<Guid> AddValeurAsync(IndicateurValeur valeur)
        {
            const string sql = @"
        INSERT INTO IndicateurValeurs (Id, IndicateurId, Periode, Valeur, Commentaire, DateMesure, SaisiParId, OrganisationId)
        VALUES (@Id, @IndId, @Periode, @Valeur, @Commentaire,@DateMesure, @SaisiParId, @OrgId)";

            valeur.Id = Guid.NewGuid();
            //valeur.DateMesure = DateTime.UtcNow;

            await _connection.ExecuteAsync(sql, new
            {
                Id = valeur.Id.ToString(),
                IndId = valeur.IndicateurId.ToString(),
                valeur.Periode,
                valeur.Valeur,
                valeur.Commentaire,
                DateMesure = valeur.DateMesure,
                valeur.SaisiParId,
                OrgId = valeur.OrganisationId.ToString()
            });

            return valeur.Id;
        }
        public async Task<bool> UpdateValeurAsync(IndicateurValeur valeur)
        {
            const string sql = @"
                UPDATE IndicateurValeurs 
                SET Valeur = @Valeur, Commentaire = @Commentaire
                WHERE Id = @Id";

            var affected = await _connection.ExecuteAsync(sql, new
            {
                Id = valeur.Id.ToString(),
                valeur.Valeur,
                valeur.Commentaire
            });

            return affected > 0;
        }

        public async Task<bool> DeleteValeurAsync(Guid id)
        {
            const string sql = "DELETE FROM IndicateurValeurs WHERE Id = @Id";
            var affected = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
            return affected > 0;
        }

        // ==================== DASHBOARD ====================

        public async Task<int> GetIndicateursCountAsync(Guid organisationId, bool? actif = null)
        {
            var sql = "SELECT COUNT(*) FROM Indicateurs WHERE OrganisationId = @OrgId";
            var parameters = new DynamicParameters();
            parameters.Add("OrgId", organisationId.ToString());

            if (actif.HasValue)
            {
                sql += " AND Actif = @Actif";
                parameters.Add("Actif", actif.Value);
            }

            return await _connection.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<decimal?> GetTendanceAsync(Guid indicateurId, int mois = 3)
        {
            var sql = @"
                SELECT (derniere.valeur - ancienne.valeur) / NULLIF(ancienne.valeur, 0) * 100 as Tendance
                FROM (SELECT Valeur FROM IndicateurValeurs WHERE IndicateurId = @IndId ORDER BY DateMesure DESC LIMIT 1) derniere
                CROSS JOIN (SELECT Valeur FROM IndicateurValeurs WHERE IndicateurId = @IndId ORDER BY DateMesure DESC LIMIT 1 OFFSET @Mois) ancienne";

            return await _connection.ExecuteScalarAsync<decimal?>(sql, new { IndId = indicateurId.ToString(), Mois = mois });
        }
    }
}
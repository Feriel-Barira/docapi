using Dapper;
using DocApi.DTOs;
using DocApi.Infrastructure;
using DocApi.Repositories.Interfaces;

namespace DocApi.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbConnectionFactory _factory;

        public DashboardRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<StatistiquesGeneralesDto> GetStatistiquesAsync(Guid organisationId)
        {
            using var conn = _factory.CreateConnection();
            var o = organisationId.ToString();

            return new StatistiquesGeneralesDto
            {
                TotalProcessus = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Processus WHERE OrganisationId=@o", new { o }),
                ProcessusActifs = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Processus WHERE OrganisationId=@o AND Statut='ACTIF'", new { o }),

                TotalProcedures = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Procedures WHERE OrganisationId=@o", new { o }),
                ProceduresActives = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Procedures WHERE OrganisationId=@o AND Statut='ACTIF'", new { o }),

                TotalDocuments = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Documents WHERE OrganisationId=@o AND Actif=TRUE", new { o }),
                DocumentsValides = await conn.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(DISTINCT d.Id) FROM Documents d
                    JOIN VersionsDocuments v ON v.DocumentId=d.Id
                    WHERE d.OrganisationId=@o AND v.Statut='VALIDE' AND d.Actif=TRUE", new { o }),
                DocumentsEnAttente = await conn.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(DISTINCT d.Id) FROM Documents d
                    JOIN VersionsDocuments v ON v.DocumentId=d.Id
                    WHERE d.OrganisationId=@o AND v.Statut IN ('BROUILLON','EN_REVISION') AND d.Actif=TRUE", new { o }),

                TotalNonConformites = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o", new { o }),
                NcOuvertes = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Statut='OUVERTE'", new { o }),
                NcAnalyse = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Statut='ANALYSE'", new { o }),
                NcActionEnCours = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Statut='ACTION_EN_COURS'", new { o }),
                NcCloturees = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Statut='CLOTUREE'", new { o }),

                TotalActionsCorrectives = await conn.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM ActionsCorrectives ac
                    JOIN NonConformites nc ON nc.Id=ac.NonConformiteId
                    WHERE nc.OrganisationId=@o", new { o }),
                ActionsEnRetard = await conn.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM ActionsCorrectives ac
                    JOIN NonConformites nc ON nc.Id=ac.NonConformiteId
                    WHERE nc.OrganisationId=@o
                      AND ac.DateEcheance < NOW()
                      AND ac.Statut NOT IN ('REALISEE','VERIFIEE')", new { o }),
                ActionsEcheanceProche = await conn.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM ActionsCorrectives ac
                    JOIN NonConformites nc ON nc.Id=ac.NonConformiteId
                    WHERE nc.OrganisationId=@o
                      AND ac.DateEcheance BETWEEN NOW() AND DATE_ADD(NOW(), INTERVAL 7 DAY)
                      AND ac.Statut NOT IN ('REALISEE','VERIFIEE')", new { o }),

                TotalIndicateurs = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Indicateurs WHERE OrganisationId=@o", new { o }),
                IndicateursActifs = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Indicateurs WHERE OrganisationId=@o AND Actif=TRUE", new { o })
            };
        }

        public async Task<RepartitionNcDto> GetRepartitionNcAsync(Guid organisationId)
        {
            using var conn = _factory.CreateConnection();
            var o = organisationId.ToString();

            return new RepartitionNcDto
            {
                Mineure        = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Gravite='MINEURE'", new { o }),
                Majeure        = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Gravite='MAJEURE'", new { o }),
                Critique       = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Gravite='CRITIQUE'", new { o }),
                ParAudit       = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Source='AUDIT'", new { o }),
                ParPointControle = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Source='POINT_CONTROLE'", new { o }),
                ParReclamation = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Source='RECLAMATION'", new { o }),
                ParAutre       = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NonConformites WHERE OrganisationId=@o AND Source='AUTRE'", new { o })
            };
        }

        public async Task<List<IndicateurResumDto>> GetIndicateursHorsCibleAsync(Guid organisationId)
        {
            using var conn = _factory.CreateConnection();
            var rows = await conn.QueryAsync<IndicateurResumDto>(@"
                SELECT i.Id, i.Nom, i.Code, i.ValeurCible, i.Unite,
                       iv.Valeur AS DerniereValeur, iv.Periode
                FROM Indicateurs i
                LEFT JOIN IndicateurValeurs iv ON iv.IndicateurId = i.Id
                    AND iv.DateMesure = (
                        SELECT MAX(iv2.DateMesure) FROM IndicateurValeurs iv2
                        WHERE iv2.IndicateurId = i.Id)
                WHERE i.OrganisationId = @o
                  AND i.Actif = TRUE
                  AND i.ValeurCible IS NOT NULL
                  AND iv.Valeur IS NOT NULL
                  AND iv.Valeur < i.ValeurCible
                ORDER BY (i.ValeurCible - iv.Valeur) DESC
                LIMIT 10", new { o = organisationId.ToString() });

            return rows.ToList();
        }
    }
}
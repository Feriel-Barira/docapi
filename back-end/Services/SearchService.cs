using Dapper;
using DocApi.DTOs;
using DocApi.Infrastructure;
using DocApi.Services.Interfaces;

namespace DocApi.Services
{
    public class SearchService : ISearchService
    {
        private readonly IDbConnectionFactory _factory;

        public SearchService(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<SearchResultDto> SearchAsync(SearchRequestDto request)
        {
            using var conn = _factory.CreateConnection();
            var t      = $"%{request.Terme.Trim()}%";
            var orgId  = request.OrganisationId;
            var result = new SearchResultDto { Terme = request.Terme };

            var tasks = new List<Task>();

            if (request.IncludeProcessus)
                tasks.Add(Task.Run(async () =>
                {
                    var rows = await conn.QueryAsync<SearchItemDto>(@"
                        SELECT Id, Code, Nom AS Titre, Description, Statut, DateCreation
                        FROM Processus
                        WHERE OrganisationId = @OrgId
                          AND (Nom LIKE @T OR Code LIKE @T OR Description LIKE @T)
                        LIMIT 10", new { OrgId = orgId, T = t });
                    result.Processus = rows.Select(r => { r.Type = "Processus"; return r; }).ToList();
                }));

            if (request.IncludeProcedures)
                tasks.Add(Task.Run(async () =>
                {
                    var rows = await conn.QueryAsync<SearchItemDto>(@"
                        SELECT Id, Code, Titre, Description, Statut, DateCreation
                        FROM Procedures
                        WHERE OrganisationId = @OrgId
                          AND (Titre LIKE @T OR Code LIKE @T OR Description LIKE @T)
                        LIMIT 10", new { OrgId = orgId, T = t });
                    result.Procedures = rows.Select(r => { r.Type = "Procedure"; return r; }).ToList();
                }));

            if (request.IncludeDocuments)
                tasks.Add(Task.Run(async () =>
                {
                    var rows = await conn.QueryAsync<SearchItemDto>(@"
                        SELECT Id, Code, Titre,
                               Description,
                               CASE WHEN Actif = 1 THEN 'ACTIF' ELSE 'INACTIF' END AS Statut,
                               DateCreation
                        FROM Documents
                        WHERE OrganisationId = @OrgId AND Actif = TRUE
                          AND (Titre LIKE @T OR Code LIKE @T OR Description LIKE @T)
                        LIMIT 10", new { OrgId = orgId, T = t });
                    result.Documents = rows.Select(r => { r.Type = "Document"; return r; }).ToList();
                }));

            if (request.IncludeNonConformites)
                tasks.Add(Task.Run(async () =>
                {
                    var rows = await conn.QueryAsync<SearchItemDto>(@"
                        SELECT Id, Reference AS Code, Description AS Titre,
                               Description, Statut, DateCreation
                        FROM NonConformites
                        WHERE OrganisationId = @OrgId
                          AND (Reference LIKE @T OR Description LIKE @T)
                        LIMIT 10", new { OrgId = orgId, T = t });
                    result.NonConformites = rows.Select(r => { r.Type = "NonConformite"; return r; }).ToList();
                }));

            if (request.IncludeIndicateurs)
                tasks.Add(Task.Run(async () =>
                {
                    var rows = await conn.QueryAsync<SearchItemDto>(@"
                        SELECT Id, Code, Nom AS Titre, Description,
                               CASE WHEN Actif = 1 THEN 'ACTIF' ELSE 'INACTIF' END AS Statut,
                               DateCreation
                        FROM Indicateurs
                        WHERE OrganisationId = @OrgId
                          AND (Nom LIKE @T OR Code LIKE @T OR Description LIKE @T)
                        LIMIT 10", new { OrgId = orgId, T = t });
                    result.Indicateurs = rows.Select(r => { r.Type = "Indicateur"; return r; }).ToList();
                }));

            await Task.WhenAll(tasks);

            result.TotalResultats =
                result.Processus.Count      +
                result.Procedures.Count     +
                result.Documents.Count      +
                result.NonConformites.Count +
                result.Indicateurs.Count;

            return result;
        }
    }
}

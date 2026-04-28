using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using System.Security.Claims;
using DocApi.DTOs;
using DocApi.Services.Interfaces;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProcessusController : ControllerBase
    {
        private readonly IDbConnection _connection;
        private readonly IAuditLogService _auditLog;

        public ProcessusController(IDbConnection connection, IAuditLogService auditLog)
        {
            _connection = connection;
            _auditLog = auditLog;
        }

        // ── Helpers pour lire le JWT ──────────────────────────────────────────────
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("id")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var uid) ? uid : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst("role")?.Value
                ?? User.FindFirst(ClaimTypes.Role)?.Value
                ?? string.Empty;
        }

        // GET /api/Processus?organisationId=xxx
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string organisationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                string sql;
                var parameters = new DynamicParameters();
                parameters.Add("OrgId", organisationId);

                if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
                {
                    // Voient TOUS les processus de l'organisation
                    sql = @"
                        SELECT p.*,
                               CONCAT(u.Prenom, ' ', u.Nom) AS PiloteNom,
                               (SELECT COUNT(*) FROM ProcessusActeurs pa WHERE pa.ProcessusId = p.Id) AS ActeursCount,
                               (SELECT COUNT(*) FROM `Procedures` pr WHERE pr.ProcessusId = p.Id) AS ProceduresCount,
                               (SELECT COUNT(*) FROM `Documents` d WHERE d.ProcessusId = p.Id) AS DocumentsCount
                        FROM Processus p
                        LEFT JOIN Users u ON p.PiloteId = u.Id
                        WHERE p.OrganisationId = @OrgId
                        ORDER BY p.DateCreation DESC";
                }
                else
                {
                    // AUDITEUR / UTILISATEUR → seulement pilote ou acteur
                    sql = @"
                        SELECT DISTINCT p.*,
                               CONCAT(u.Prenom, ' ', u.Nom) AS PiloteNom,
                               (SELECT COUNT(*) FROM ProcessusActeurs pa2 WHERE pa2.ProcessusId = p.Id) AS ActeursCount,
                               (SELECT COUNT(*) FROM `Procedures` pr WHERE pr.ProcessusId = p.Id) AS ProceduresCount,
                               (SELECT COUNT(*) FROM `Documents` d WHERE d.ProcessusId = p.Id) AS DocumentsCount
                        FROM Processus p
                        LEFT JOIN Users u ON p.PiloteId = u.Id
                        LEFT JOIN ProcessusActeurs pa ON pa.ProcessusId = p.Id AND pa.UtilisateurId = @UserId
                        WHERE p.OrganisationId = @OrgId
                          AND (p.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL)
                        ORDER BY p.DateCreation DESC";
                    parameters.Add("UserId", userId);
                }

                var result = await _connection.QueryAsync(sql, parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET /api/Processus/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var sql = @"
                    SELECT p.*,
                           CONCAT(u.Prenom, ' ', u.Nom) AS PiloteNom
                    FROM Processus p
                    LEFT JOIN Users u ON p.PiloteId = u.Id
                    WHERE p.Id = @Id";
                var result = await _connection.QueryFirstOrDefaultAsync(sql, new { Id = id });

                if (result == null)
                    return NotFound(new { message = "Processus non trouvé" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET /api/Processus/filter
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] string organisationId,
            [FromQuery] string? searchTerm,
            [FromQuery] string? type,
            [FromQuery] string? statut,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                string sql;
                var parameters = new DynamicParameters();
                parameters.Add("OrgId", organisationId);

                if (userRole == "ADMIN_ORG" || userRole == "RESPONSABLE_SMQ")
                {
                    sql = @"
                        SELECT p.*,
                               CONCAT(u.Prenom, ' ', u.Nom) AS PiloteNom
                        FROM Processus p
                        LEFT JOIN Users u ON p.PiloteId = u.Id
                        WHERE p.OrganisationId = @OrgId";
                }
                else
                {
                    sql = @"
                        SELECT DISTINCT p.*,
                               CONCAT(u.Prenom, ' ', u.Nom) AS PiloteNom
                        FROM Processus p
                        LEFT JOIN Users u ON p.PiloteId = u.Id
                        LEFT JOIN ProcessusActeurs pa ON pa.ProcessusId = p.Id AND pa.UtilisateurId = @UserId
                        WHERE p.OrganisationId = @OrgId
                          AND (p.PiloteId = @UserId OR pa.UtilisateurId IS NOT NULL)";
                    parameters.Add("UserId", userId);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " AND (p.Nom LIKE @SearchTerm OR p.Code LIKE @SearchTerm)";
                    parameters.Add("SearchTerm", $"%{searchTerm}%");
                }
                if (!string.IsNullOrWhiteSpace(type))
                {
                    sql += " AND p.Type = @Type";
                    parameters.Add("Type", type);
                }
                if (!string.IsNullOrWhiteSpace(statut))
                {
                    sql += " AND p.Statut = @Statut";
                    parameters.Add("Statut", statut);
                }

                sql += " ORDER BY p.DateCreation DESC LIMIT @Offset, @PageSize";
                parameters.Add("Offset", (page - 1) * pageSize);
                parameters.Add("PageSize", pageSize);

                var result = await _connection.QueryAsync(sql, parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET /api/Processus/{id}/acteurs
        [HttpGet("{id}/acteurs")]
        public async Task<IActionResult> GetActeurs(string id)
        {
            try
            {
                var sql = @"
                    SELECT pa.*, CONCAT(u.Prenom, ' ', u.Nom) AS NomComplet, u.Email
                    FROM ProcessusActeurs pa
                    LEFT JOIN Users u ON pa.UtilisateurId = u.Id
                    WHERE pa.ProcessusId = @ProcessusId";
                var result = await _connection.QueryAsync(sql, new { ProcessusId = id });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST /api/Processus?organisationId=xxx
        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] string organisationId, [FromBody] CreateProcessusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var id = Guid.NewGuid().ToString();

                var sql = @"
                    INSERT INTO Processus (Id, OrganisationId, Code, Nom, Description, Type, PiloteId, Statut,
                                          Finalites, Objectifs, Perimetres, Fournisseurs, Clients, DonneesEntree, DonneesSortie, DateCreation)
                    VALUES (@Id, @OrgId, @Code, @Nom, @Description, @Type, @PiloteId, @Statut,
                            @Finalites, @Objectifs, @Perimetres, @Fournisseurs, @Clients, @DonneesEntree, @DonneesSortie, NOW())";

                await _connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    OrgId = organisationId,
                    dto.Code,
                    dto.Nom,
                    dto.Description,
                    Type = dto.Type,
                    PiloteId = dto.PiloteId,
                    Statut = dto.Statut,
                    Finalites = System.Text.Json.JsonSerializer.Serialize(dto.Finalites ?? new List<string>()),
                    Objectifs = System.Text.Json.JsonSerializer.Serialize(dto.Objectifs ?? new List<string>()),
                    Perimetres = System.Text.Json.JsonSerializer.Serialize(dto.Perimetres ?? new List<string>()),
                    Fournisseurs = System.Text.Json.JsonSerializer.Serialize(dto.Fournisseurs ?? new List<string>()),
                    Clients = System.Text.Json.JsonSerializer.Serialize(dto.Clients ?? new List<string>()),
                    DonneesEntree = System.Text.Json.JsonSerializer.Serialize(dto.DonneesEntree ?? new List<string>()),
                    DonneesSortie = System.Text.Json.JsonSerializer.Serialize(dto.DonneesSortie ?? new List<string>())
                });

                if (dto.Acteurs != null && dto.Acteurs.Any())
                {
                    foreach (var acteur in dto.Acteurs)
                    {
                        var acteurId = Guid.NewGuid().ToString();
                        await _connection.ExecuteAsync(@"
                            INSERT INTO ProcessusActeurs (Id, ProcessusId, UtilisateurId, TypeActeur, OrganisationId, DateAffectation)
                            VALUES (@Id, @ProcessusId, @UtilisateurId, @TypeActeur, @OrgId, NOW())",
                            new
                            {
                                Id = acteurId,
                                ProcessusId = id,
                                acteur.UtilisateurId,
                                TypeActeur = acteur.TypeActeur,
                                OrgId = organisationId
                            });
                    }
                }

                await _auditLog.LogAsync("CREATE_PROCESSUS", "Processus", id, "Processus créé");
                return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Processus créé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT /api/Processus/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateProcessusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var sql = @"
                    UPDATE Processus
                    SET Nom              = COALESCE(@Nom, Nom),
                        Code             = COALESCE(@Code, Code),
                        Description      = COALESCE(@Description, Description),
                        Type             = COALESCE(@Type, Type),
                        Statut           = COALESCE(@Statut, Statut),
                        PiloteId         = COALESCE(@PiloteId, PiloteId),
                        Finalites        = COALESCE(@Finalites, Finalites),
                        Objectifs        = COALESCE(@Objectifs, Objectifs),
                        Perimetres       = COALESCE(@Perimetres, Perimetres),
                        Fournisseurs     = COALESCE(@Fournisseurs, Fournisseurs),
                        Clients          = COALESCE(@Clients, Clients),
                        DonneesEntree    = COALESCE(@DonneesEntree, DonneesEntree),
                        DonneesSortie    = COALESCE(@DonneesSortie, DonneesSortie),
                        DateModification = NOW()
                    WHERE Id = @Id";

                var affected = await _connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    dto.Code,
                    dto.Nom,
                    dto.Description,
                    dto.Type,
                    dto.Statut,
                    dto.PiloteId,
                    Finalites = dto.Finalites != null ? System.Text.Json.JsonSerializer.Serialize(dto.Finalites) : null,
                    Objectifs = dto.Objectifs != null ? System.Text.Json.JsonSerializer.Serialize(dto.Objectifs) : null,
                    Perimetres = dto.Perimetres != null ? System.Text.Json.JsonSerializer.Serialize(dto.Perimetres) : null,
                    Fournisseurs = dto.Fournisseurs != null ? System.Text.Json.JsonSerializer.Serialize(dto.Fournisseurs) : null,
                    Clients = dto.Clients != null ? System.Text.Json.JsonSerializer.Serialize(dto.Clients) : null,
                    DonneesEntree = dto.DonneesEntree != null ? System.Text.Json.JsonSerializer.Serialize(dto.DonneesEntree) : null,
                    DonneesSortie = dto.DonneesSortie != null ? System.Text.Json.JsonSerializer.Serialize(dto.DonneesSortie) : null
                });

                if (affected == 0)
                    return NotFound(new { message = "Processus non trouvé" });

                if (dto.Acteurs != null)
                {
                    await _connection.ExecuteAsync(
                        "DELETE FROM ProcessusActeurs WHERE ProcessusId = @Id", new { Id = id });

                    var orgId = await _connection.ExecuteScalarAsync<string>(
                        "SELECT CAST(OrganisationId AS CHAR) FROM Processus WHERE Id = @Id", new { Id = id });

                    foreach (var acteur in dto.Acteurs)
                    {
                        var acteurId = Guid.NewGuid().ToString();
                        await _connection.ExecuteAsync(@"
                            INSERT INTO ProcessusActeurs (Id, ProcessusId, UtilisateurId, TypeActeur, OrganisationId, DateAffectation)
                            VALUES (@Id, @ProcessusId, @UtilisateurId, @TypeActeur, @OrgId, NOW())",
                            new { Id = acteurId, ProcessusId = id, acteur.UtilisateurId, TypeActeur = acteur.TypeActeur, OrgId = orgId });
                    }
                }

                await _auditLog.LogAsync("UPDATE_PROCESSUS", "Processus", id, "Processus modifié");
                return Ok(new { message = "Mis à jour avec succès" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE /api/Processus/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _connection.ExecuteAsync(
                    "DELETE FROM ProcessusActeurs WHERE ProcessusId = @Id", new { Id = id });

                var affected = await _connection.ExecuteAsync(
                    "DELETE FROM Processus WHERE Id = @Id", new { Id = id });

                if (affected == 0)
                    return NotFound(new { message = "Processus non trouvé" });

                await _auditLog.LogAsync("DELETE_PROCESSUS", "Processus", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE /api/Processus/acteurs/{acteurId}
        [HttpDelete("acteurs/{acteurId}")]
        public async Task<IActionResult> RemoveActeur(string acteurId)
        {
            try
            {
                var affected = await _connection.ExecuteAsync(
                    "DELETE FROM ProcessusActeurs WHERE Id = @Id", new { Id = acteurId });

                if (affected == 0)
                    return NotFound(new { message = "Acteur non trouvé" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET /api/Processus/database-info
        [HttpGet("database-info")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var dbName = await _connection.ExecuteScalarAsync<string>("SELECT DATABASE()");
                var total = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Processus");
                return Ok(new { database = dbName, totalProcessus = total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET /api/Processus/check-code
        [HttpGet("check-code")]
        public async Task<IActionResult> CheckCodeUniqueness(
            [FromQuery] string code,
            [FromQuery] string organisationId,
            [FromQuery] string? excludeId = null)
        {
            try
            {
                var sql = "SELECT COUNT(1) FROM Processus WHERE Code = @Code AND OrganisationId = @OrgId";
                var parameters = new DynamicParameters();
                parameters.Add("Code", code);
                parameters.Add("OrgId", organisationId);

                if (!string.IsNullOrEmpty(excludeId))
                {
                    sql += " AND Id != @ExcludeId";
                    parameters.Add("ExcludeId", excludeId);
                }

                var count = await _connection.ExecuteScalarAsync<int>(sql, parameters);
                return Ok(new { exists = count > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
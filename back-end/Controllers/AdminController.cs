using Dapper;
using DocApi.DTOs;
using DocApi.Infrastructure;
using DocApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]   
    public class AdminController : ControllerBase
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IAuditLogService _auditLog;  

        public AdminController(IDbConnectionFactory connectionFactory, IAuditLogService auditLog)
        {
            _connectionFactory = connectionFactory;
            _auditLog = auditLog;
        }

        // ── Liste des utilisateurs ── NOUVEAU ──────────────────────────────────
        [HttpGet("users")]
        // [AllowAnonymous]
        public async Task<IActionResult> GetUsers()
        {
            using var conn = _connectionFactory.CreateConnection();
            var users = await conn.QueryAsync(
                @"SELECT Id, Username, Email, 
                 RoleGlobal,   -- ← on prend roleGlobal
                 CAST(OrganisationId AS CHAR) AS OrganisationId,
                 Nom, Prenom, Fonction, CreatedAt, IsActive
          FROM Users");
            return Ok(users);
        }

        // ── Activer / Désactiver un compte ── NOUVEAU ──────────────────────────
        [HttpPut("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            using var conn = _connectionFactory.CreateConnection();
            var affected = await conn.ExecuteAsync(
                "UPDATE Users SET IsActive = NOT IsActive WHERE Id = @Id", new { Id = id });

            if (affected == 0) return NotFound(new { message = "Utilisateur introuvable." });

            await _auditLog.LogAsync("TOGGLE_USER", "User", id.ToString());
            return Ok(new { message = "Statut mis à jour." });
        }

        // ── Changer le rôle ── NOUVEAU ─────────────────────────────────────────
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] string role)
        {
            var allowed = new[] { "ADMIN_ORG", "UTILISATEUR", "RESPONSABLE_SMQ", "AUDITEUR" };
            if (!allowed.Contains(role))
                return BadRequest(new { message = $"Rôle invalide. Valeurs acceptées : {string.Join(", ", allowed)}" });

            using var conn = _connectionFactory.CreateConnection();
            var affected = await conn.ExecuteAsync(
                "UPDATE Users SET Role = @Role WHERE Id = @Id", new { Role = role, Id = id });

            if (affected == 0) return NotFound(new { message = "Utilisateur introuvable." });

            await _auditLog.LogAsync("CHANGE_ROLE", "User", id.ToString(), $"Nouveau rôle : {role}");
            return Ok(new { message = "Rôle mis à jour." });
        }

        // ── Journal d'audit ── NOUVEAU ─────────────────────────────────────────
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
        {
            var result = await _auditLog.GetFilteredAsync(filter);
            return Ok(result);
        }

        // ── Test auth Admin ────────────────────────────────────────────────────
        [HttpGet("test")]
        public IActionResult Test()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            return Ok(new { message = $"Admin OK — connecté en tant que {username}" });
        }

        // ── Utilitaires dev (conservés) ────────────────────────────────────────
        [HttpPost("update-password-hashes")]
        public async Task<IActionResult> UpdatePasswordHashes()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                var userHash = BCrypt.Net.BCrypt.HashPassword("user123");
                var managerHash = BCrypt.Net.BCrypt.HashPassword("manager123");

                var r1 = await connection.ExecuteAsync("UPDATE Users SET PasswordHash=@H WHERE Username=@U", new { H = adminHash, U = "admin" });
                var r2 = await connection.ExecuteAsync("UPDATE Users SET PasswordHash=@H WHERE Username=@U", new { H = userHash, U = "user1" });
                var r3 = await connection.ExecuteAsync("UPDATE Users SET PasswordHash=@H WHERE Username=@U", new { H = managerHash, U = "manager" });

                return Ok(new { admin = r1 > 0 ? "Updated" : "Not found", user1 = r2 > 0 ? "Updated" : "Not found", manager = r3 > 0 ? "Updated" : "Not found" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("verify-users")]
        public async Task<IActionResult> VerifyUsers()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var users = await connection.QueryAsync(
                    "SELECT Username, Email, Role, LEFT(PasswordHash,30) as PasswordHashPreview FROM Users WHERE IsActive=1");
                return Ok(users);
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }
        // ==================== AJOUTER DANS AdminController.cs ====================

        

        // POST: api/Admin/users → créer un utilisateur
        [HttpPost("users")]
        [AllowAnonymous]  // à retirer plus tard
        public async Task<IActionResult> CreateUser([FromQuery] Guid organisationId, [FromBody] CreateUserRequest request)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            using var conn = _connectionFactory.CreateConnection();

            // Vérifier doublons
            var exists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Users WHERE Email = @Email OR Username = @Username",
                new { request.Email, request.Username }) > 0;
            if (exists)
                return Conflict(new { message = "Email ou nom d'utilisateur déjà utilisé." });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var sql = @"
        INSERT INTO Users (Id, OrganisationId, Username, Email, PasswordHash, Role, RoleGlobal, Nom, Prenom, Fonction, IsActive, CreatedAt)
        VALUES (UUID(), @OrgId, @Username, @Email, @PasswordHash, 'User', @RoleGlobal, @Nom, @Prenom, @Fonction, @IsActive, NOW())";
            await conn.ExecuteAsync(sql, new
            {
                OrgId = organisationId.ToString(),
                request.Username,
                request.Email,
                PasswordHash = passwordHash,
                request.RoleGlobal,
                request.Nom,
                request.Prenom,
                request.Fonction,
                request.IsActive
            });

            await _auditLog.LogAsync("CREATE_USER", "User", null, $"Utilisateur {request.Username} créé");
            return Ok(new { message = "Utilisateur créé." });
        }

        // PUT: api/Admin/users/{id} → modifier un utilisateur
        [HttpPut("users/{id}")]
        [AllowAnonymous] // À retirer plus tard
        public async Task<IActionResult> UpdateUser(int id, [FromQuery] Guid organisationId, [FromBody] UpdateUserRequest request)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            using var conn = _connectionFactory.CreateConnection();

            // Vérifier que l'utilisateur appartient bien à l'organisation (optionnel, mais recommandé)
            var belongs = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Users WHERE Id = @Id AND OrganisationId = @OrgId",
                new { Id = id, OrgId = organisationId.ToString() }) > 0;
            if (!belongs)
                return NotFound(new { message = "Utilisateur non trouvé dans cette organisation." });

            var sql = @"
        UPDATE Users
        SET Nom = @Nom,
            Prenom = @Prenom,
            Email = @Email,
            Username = @Username,
            RoleGlobal = @RoleGlobal,
            Fonction = @Fonction,
            IsActive = @IsActive
        WHERE Id = @Id";
            await conn.ExecuteAsync(sql, new
            {
                Id = id,
                request.Nom,
                request.Prenom,
                request.Email,
                request.Username,
                request.RoleGlobal,
                request.Fonction,
                request.IsActive
            });

            await _auditLog.LogAsync("UPDATE_USER", "User", id.ToString());
            return Ok(new { message = "Utilisateur mis à jour." });
        }

        // DELETE: api/Admin/users/{id} → supprimer un utilisateur
        [HttpDelete("users/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(int id, [FromQuery] Guid organisationId)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            using var conn = _connectionFactory.CreateConnection();

            var exists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Users WHERE Id = @Id AND OrganisationId = @OrgId",
                new { Id = id, OrgId = organisationId.ToString() }) > 0;
            if (!exists) return NotFound();

            await conn.ExecuteAsync("DELETE FROM Users WHERE Id = @Id", new { Id = id });
            await _auditLog.LogAsync("DELETE_USER", "User", id.ToString());
            return NoContent();
        }

        // Ajoute cette méthode pour récupérer l'OrganisationId depuis le token
        private Guid GetCurrentOrganisationId()
        {
            var claim = User.FindFirst("OrganisationId")?.Value;
            return Guid.TryParse(claim, out var orgId) ? orgId : Guid.Empty;
        }
    }
}

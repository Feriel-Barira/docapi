using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using DocApi.DTOs;
using DocApi.Services.Interfaces;
using System.Security.Claims;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IndicateurController : ControllerBase
    {
        private readonly IIndicateurService _service;
        private readonly IDbConnection _connection;

        public IndicateurController(IIndicateurService service, IDbConnection connection)
        {
            _service = service;
            _connection = connection;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }

        // ==================== DROPDOWNS ====================

        [HttpGet("processus-list")]
        public async Task<IActionResult> GetProcessusList([FromQuery] Guid organisationId)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            var sql = @"
                SELECT Id, Code, Nom
                FROM Processus 
                WHERE OrganisationId = @OrganisationId 
                ORDER BY Code";

            var processus = await _connection.QueryAsync(sql, new { OrganisationId = organisationId.ToString() });
            return Ok(processus);
        }

        [HttpGet("responsables")]
        public async Task<IActionResult> GetResponsables([FromQuery] Guid organisationId)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            var sql = @"
        SELECT Id, CONCAT(Prenom, ' ', Nom) AS Nom
        FROM Users 
        WHERE OrganisationId = @OrganisationId 
        AND IsActive = 1
        AND RoleGlobal != 'UTILISATEUR'
        ORDER BY Nom";

            var responsables = await _connection.QueryAsync(sql, new { OrganisationId = organisationId.ToString() });
            return Ok(responsables);
        }

        // ==================== INDICATEURS ====================

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid organisationId)
        {
            var result = await _service.GetAllAsync(organisationId);
            return Ok(result);
        }

        [HttpGet("processus/{processusId}")]
        public async Task<IActionResult> GetByProcessus(Guid processusId)
        {
            var result = await _service.GetByProcessusAsync(processusId);
            return Ok(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] Guid organisationId, [FromQuery] IndicateurFilterDto filter)
        {
            var result = await _service.GetFilteredAsync(organisationId, filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Indicateur non trouvé" });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] Guid organisationId, [FromBody] CreateIndicateurDto dto)
        {
            try
            {
                Console.WriteLine($"=== CREATE INDICATEUR ===");
                Console.WriteLine($"OrganisationId: {organisationId}");
                Console.WriteLine($"Code: {dto.Code}");
                Console.WriteLine($"Nom: {dto.Nom}");
                Console.WriteLine($"ProcessusId: {dto.ProcessusId}");
                Console.WriteLine($"ResponsableId: {dto.ResponsableId}");
                Console.WriteLine($"FrequenceMesure: {dto.FrequenceMesure}");
                // Vérifier que le processusId est fourni
                if (!dto.ProcessusId.HasValue || dto.ProcessusId.Value == Guid.Empty)
                    return BadRequest(new { message = "Le processus est obligatoire" });

               
                var result = await _service.CreateAsync(organisationId, dto.ProcessusId.Value, dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIndicateurDto dto)
        {
            Console.WriteLine($"=== UPDATE RECEIVED ===");  // ← Premier log
            Console.WriteLine($"ID: {id}");
            try
            {
                // 🔍 LOGS DÉTAILLÉS
                Console.WriteLine($"=== UPDATE INDICATEUR ===");
                Console.WriteLine($"ID: {id}");
                Console.WriteLine($"DTO reçu - Code: {dto.Code}");
                Console.WriteLine($"DTO reçu - Nom: {dto.Nom}");
                Console.WriteLine($"DTO reçu - Description: {dto.Description}");
                Console.WriteLine($"DTO reçu - MethodeCalcul: {dto.MethodeCalcul}");
                Console.WriteLine($"DTO reçu - Unite: {dto.Unite}");
                Console.WriteLine($"DTO reçu - ValeurCible: {dto.ValeurCible}");
                Console.WriteLine($"DTO reçu - SeuilAlerte: {dto.SeuilAlerte}");
                Console.WriteLine($"DTO reçu - FrequenceMesure: {dto.FrequenceMesure}");
                Console.WriteLine($"DTO reçu - ResponsableId: {dto.ResponsableId}");
                Console.WriteLine($"DTO reçu - Actif: {dto.Actif}");
                Console.WriteLine($"DTO reçu - ProcessusId: {dto.ProcessusId}");

                var result = await _service.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"ERREUR NotFound: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"ERREUR InvalidOperation: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR Générale: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpPatch("{id}/statut")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateStatut(Guid id, [FromBody] UpdateStatutRequest request)
        {
            try
            {
                Console.WriteLine($"=== PATCH Statut ===");
                Console.WriteLine($"ID: {id}");
                Console.WriteLine($"Request reçu: {request != null}");
                if (request != null)
                {
                    Console.WriteLine($"Actif: {request.Actif}");
                }
                else
                {
                    Console.WriteLine("ERREUR: Request est null!");
                    return BadRequest(new { message = "Données invalides" });
                }

                var dto = new UpdateIndicateurDto { Actif = request.Actif };
                var result = await _service.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                return result ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ==================== VALEURS ====================

        [HttpGet("{id}/valeurs")]
        public async Task<IActionResult> GetValeurs(Guid id, [FromQuery] int limit = 12)
        {
            var result = await _service.GetValeursAsync(id, limit);
            return Ok(result);
        }

        [HttpPost("{id}/valeurs")]
        public async Task<IActionResult> AddValeur(Guid id, [FromBody] CreateIndicateurValeurDto dto)
        {
            try
            {
                var userId = dto.SaisiParId > 0 ? dto.SaisiParId : GetCurrentUserId();
                var result = await _service.AddValeurAsync(id, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("valeurs/{valeurId}")]
        public async Task<IActionResult> UpdateValeur(Guid valeurId, [FromBody] CreateIndicateurValeurDto dto)
        {
            try
            {
                var result = await _service.UpdateValeurAsync(valeurId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("valeurs/{valeurId}")]
        public async Task<IActionResult> DeleteValeur(Guid valeurId)
        {
            var result = await _service.DeleteValeurAsync(valeurId);
            return result ? NoContent() : NotFound();
        }
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            Console.WriteLine("=== TEST ENDPOINT HIT ===");
            return Ok(new { message = "Test réussi" });
        }

    }
}
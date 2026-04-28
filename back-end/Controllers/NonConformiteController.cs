// Controllers/NonConformiteController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocApi.DTOs;
using DocApi.Services.Interfaces;
using System.Security.Claims;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NonConformiteController : ControllerBase
    {
        private readonly INonConformiteService _service;
        private readonly IAuditLogService _auditLog;

        public NonConformiteController(INonConformiteService service, IAuditLogService auditLog)
        {
            _service = service;
            _auditLog = auditLog;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid organisationId)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst("role")?.Value
                        ?? User.FindFirst(ClaimTypes.Role)?.Value
                        ?? string.Empty;
            var result = await _service.GetAllAsync(organisationId, userRole, userId);
            return Ok(result);
        }

        [HttpGet("processus/{processusId}")]
        public async Task<IActionResult> GetByProcessus(Guid processusId)
        {
            var result = await _service.GetByProcessusAsync(processusId);
            return Ok(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] Guid organisationId, [FromQuery] NonConformiteFilterDto filter)
        {
            var result = await _service.GetFilteredAsync(organisationId, filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Non-conformité non trouvée" });
            return Ok(result);
        }

        [HttpGet("{id}/historique")]
        public async Task<IActionResult> GetHistorique(Guid id)
        {
            var result = await _service.GetHistoriqueAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] Guid organisationId, [FromBody] CreateNonConformiteDto dto)
        {
            Console.WriteLine($"=== CREATE NC ===");
            Console.WriteLine($"ResponsableTraitementId reçu: {dto.ResponsableTraitementId}");
            Console.WriteLine($"DetecteParId reçu: {dto.DetecteParId}");
            Console.WriteLine($"DateDetection reçu: {dto.DateDetection}");
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.CreateAsync(organisationId, dto, userId);
                await _auditLog.LogAsync("CREATE_NC", "NonConformite", result.Id.ToString(), $"NC {result.Reference} créée");  // ← AJOUTER
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNonConformiteDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                await _auditLog.LogAsync("UPDATE_NC", "NonConformite", id.ToString());  // ← AJOUTER
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/statut")]
        public async Task<IActionResult> UpdateStatut(Guid id, [FromQuery] string statut, [FromBody] string? commentaire)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UpdateStatutAsync(id, statut, userId, commentaire);
                await _auditLog.LogAsync("UPDATE_NC", "NonConformite", id.ToString(), $"Statut changé → {statut}");  // ← AJOUTER
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                await _auditLog.LogAsync("DELETE_NC", "NonConformite", id.ToString());  // ← AJOUTER
                return result ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ==================== ANALYSES ====================

        [HttpPost("{id}/analyse")]
        public async Task<IActionResult> AddAnalyse(Guid id, [FromBody] CreateAnalyseCauseDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.AddAnalyseAsync(id, dto, userId);
                return Ok(result);
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

        [HttpPut("analyse/{analyseId}")]
        public async Task<IActionResult> UpdateAnalyse(Guid analyseId, [FromBody] CreateAnalyseCauseDto dto)
        {
            try
            {
                var result = await _service.UpdateAnalyseAsync(analyseId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
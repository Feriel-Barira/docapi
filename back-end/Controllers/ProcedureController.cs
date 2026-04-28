// Controllers/ProcedureController.cs
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
    public class ProcedureController : ControllerBase
    {
        private readonly IProcedureService _service;
        private readonly IAuditLogService _auditLog;

        public ProcedureController(IProcedureService service, IAuditLogService auditLog)
        {
            _service = service;
            _auditLog = auditLog;
        }

        // ==================== PROCÉDURES ====================

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid organisationId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _service.GetAllAsync(organisationId, userRole, userId);
            return Ok(result);
        }

        [HttpGet("processus/{processusId}")]
        public async Task<IActionResult> GetByProcessus(Guid processusId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _service.GetByProcessusAsync(processusId, userRole, userId);
            return Ok(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] Guid organisationId, [FromQuery] ProcedureFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _service.GetFilteredAsync(organisationId, userRole, userId, filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Procédure non trouvée" });
            return Ok(result);
        }

        [HttpPost("{processusId}")]
        public async Task<IActionResult> Create(Guid processusId, [FromQuery] Guid organisationId, [FromBody] CreateProcedureDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.CreateAsync(organisationId, processusId, dto);
                await _auditLog.LogAsync("CREATE_PROCEDURE", "Procedure", result.Id.ToString(), $"Procédure {result.Code} créée");  // ← AJOUTER
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProcedureDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                await _auditLog.LogAsync("UPDATE_PROCEDURE", "Procedure", id.ToString());  
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                await _auditLog.LogAsync("DELETE_PROCEDURE", "Procedure", id.ToString());  
                return result ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ==================== INSTRUCTIONS ====================

        [HttpGet("{id}/instructions")]
        public async Task<IActionResult> GetInstructions(Guid id)
        {
            var result = await _service.GetInstructionsAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/instructions")]
        public async Task<IActionResult> AddInstruction(Guid id, [FromBody] CreateInstructionDto dto)
        {
            try
            {
                var result = await _service.AddInstructionAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("instructions/{instructionId}")]
        public async Task<IActionResult> UpdateInstruction(Guid instructionId, [FromBody] UpdateInstructionDto dto)
        {
            try
            {
                var result = await _service.UpdateInstructionAsync(instructionId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("instructions/{instructionId}")]
        public async Task<IActionResult> DeleteInstruction(Guid instructionId)
        {
            var result = await _service.DeleteInstructionAsync(instructionId);
            return result ? NoContent() : NotFound();
        }
        [HttpGet("check-code")]
        public async Task<IActionResult> CheckCodeUniqueness(
    [FromQuery] string code,
    [FromQuery] Guid organisationId,
    [FromQuery] Guid? excludeId = null)
        {
            var exists = await _service.CodeExistsAsync(code, organisationId, excludeId);
            return Ok(new { exists });
        }
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
    }
}
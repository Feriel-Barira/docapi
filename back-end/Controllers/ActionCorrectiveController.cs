// Controllers/ActionCorrectiveController.cs
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
    public class ActionCorrectiveController : ControllerBase
    {
        private readonly IActionCorrectiveService _service;

        public ActionCorrectiveController(IActionCorrectiveService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }

        [HttpGet("nonconformite/{nonConformiteId}")]
        public async Task<IActionResult> GetByNonConformite(Guid nonConformiteId)
        {
            var result = await _service.GetByNonConformiteAsync(nonConformiteId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Action corrective non trouvée" });
            return Ok(result);
        }

        [HttpGet("echeance-proche")]
        public async Task<IActionResult> GetEcheanceProche([FromQuery] Guid organisationId, [FromQuery] int joursAlerte = 7)
        {
            var result = await _service.GetEcheanceProcheAsync(organisationId, joursAlerte);
            return Ok(result);
        }

        [HttpPost("nonconformite/{nonConformiteId}")]
        public async Task<IActionResult> Create(Guid nonConformiteId, [FromBody] CreateActionCorrectiveDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(nonConformiteId, dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActionCorrectiveDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/realiser")]
        public async Task<IActionResult> Realiser(Guid id, [FromBody] string commentaire)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.RealiserAsync(id, commentaire, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/verifier")]
        public async Task<IActionResult> Verifier(Guid id, [FromBody] VerifierActionCorrectiveDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.VerifierAsync(id, dto.Efficace, dto.Commentaire, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
        [HttpPut("{id}/preuve")]
        public async Task<IActionResult> AttacherPreuve(Guid id, [FromBody] Guid enregistrementId)
        {
            await _service.AttacherPreuveAsync(id, enregistrementId);
            return Ok();
        }
        [HttpDelete("{id}/preuve")]
        public async Task<IActionResult> DetacherPreuve(Guid id)
        {
            await _service.DetacherPreuveAsync(id);
            return Ok();
        }
    }
}
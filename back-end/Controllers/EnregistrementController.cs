using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocApi.DTOs;
using DocApi.Services.Interfaces;
using System.Security.Claims;
using System.IO;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnregistrementController : ControllerBase
    {
        private readonly IEnregistrementService _service;
        private readonly IAuditLogService _auditLog;

        public EnregistrementController(IEnregistrementService service, IAuditLogService auditLog)
        {
            _service = service;
            _auditLog = auditLog;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? processusId)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst("role")?.Value
                        ?? User.FindFirst(ClaimTypes.Role)?.Value
                        ?? string.Empty;
            var result = await _service.GetAllAsync(processusId, userRole, userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEnregistrementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // OrganisationId est envoyé par le front (localStorage)
                var result = await _service.CreateAsync(dto, GetCurrentUserId(), dto.OrganisationId);
                await _auditLog.LogAsync("CREATE_ENREGISTREMENT", "Enregistrement", result.Id.ToString(), $"Preuve ajoutée pour le processus {dto.ProcessusId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var (stream, contentType, fileName) = await _service.GetFileAsync(id);
                return File(stream, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            await _auditLog.LogAsync("DELETE_ENREGISTREMENT", "Enregistrement", id.ToString());
            return NoContent();
        }
    }
}
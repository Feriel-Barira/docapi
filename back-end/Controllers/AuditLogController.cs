using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocApi.DTOs;
using DocApi.Services.Interfaces;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditLogController(IAuditLogService service)
        {
            _service = service;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 50)
        {
            var result = await _service.GetRecentAsync(count);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFiltered([FromQuery] AuditLogFilterDto filter)
        {
            var result = await _service.GetFilteredAsync(filter);
            return Ok(result);
        }
    }
}
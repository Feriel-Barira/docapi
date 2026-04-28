using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocApi.DTOs;
using DocApi.Services.Interfaces;

namespace DocApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PointControleController : ControllerBase
    {
        private readonly IPointControleService _pointControleService;
        private readonly IDbConnection _connection;

        public PointControleController(IPointControleService pointControleService, IDbConnection connection)
        {
            _pointControleService = pointControleService;
            _connection = connection;
        }

        // GET: api/PointControle?organisationId={id}
        [HttpGet]
        public async Task<IActionResult> GetByOrganisationId([FromQuery] Guid organisationId)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            var result = await _pointControleService.GetByOrganisationIdAsync(organisationId);
            return Ok(result);
        }

        // GET: api/PointControle/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _pointControleService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Point de contrôle non trouvé" });
            return Ok(result);
        }

        // GET: api/PointControle/responsables?organisationId={id}
        [HttpGet("responsables")]
        public async Task<IActionResult> GetResponsables([FromQuery] Guid organisationId)
        {
            var sql = @"
        SELECT DISTINCT u.Id, CONCAT(u.Prenom, ' ', u.Nom) AS Nom
        FROM Users u
        LEFT JOIN ProcessusActeurs pa ON pa.UtilisateurId = u.Id
        WHERE u.OrganisationId = @OrganisationId 
        AND u.IsActive = 1
        AND (
            u.RoleGlobal IN ('ADMIN_ORG', 'RESPONSABLE_SMQ', 'CONTRIBUTEUR')  -- ← AJOUTER CONTRIBUTEUR ICI
            OR pa.TypeActeur IN ('PILOTE', 'COPILOTE', 'CONTRIBUTEUR')
        )
        ORDER BY Nom";

            var responsables = await _connection.QueryAsync(sql, new { OrganisationId = organisationId.ToString() });
            return Ok(responsables);
        }
        // GET: api/PointControle/users?organisationId={id}
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] Guid organisationId)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            var sql = @"
        SELECT Id, CONCAT(Prenom, ' ', Nom) AS Nom
        FROM Users 
        WHERE OrganisationId = @OrganisationId 
        AND IsActive = 1
        AND RoleGlobal IN ('ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR')
        ORDER BY Nom";

            var users = await _connection.QueryAsync(sql, new { OrganisationId = organisationId.ToString() });
            return Ok(users);
        }

        // POST: api/PointControle?organisationId={id}
        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] Guid organisationId, [FromBody] PointControleCreateDto dto)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });

            if (string.IsNullOrWhiteSpace(dto.Nom))
                return BadRequest(new { message = "Le nom est obligatoire" });

            var result = await _pointControleService.CreateAsync(organisationId, dto);
            return Ok(result);
        }

        // PUT: api/PointControle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromQuery] Guid organisationId, [FromBody] PointControleCreateDto dto)
        {
            if (organisationId == Guid.Empty)
                return BadRequest(new { message = "OrganisationId est requis" });
            if (string.IsNullOrWhiteSpace(dto.Nom))
                return BadRequest(new { message = "Le nom est obligatoire" });

            var result = await _pointControleService.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(new { message = "Point de contrôle non trouvé" });
            return Ok(result);
        }

        // DELETE: api/PointControle/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _pointControleService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Point de contrôle non trouvé" });
            return NoContent();
        }

        // GET: api/PointControle/{id}/evaluations
        [HttpGet("{id}/evaluations")]
        public async Task<IActionResult> GetEvaluations(Guid id)
        {
            var result = await _pointControleService.GetEvaluationsAsync(id);
            return Ok(result);
        }

        // POST: api/PointControle/{id}/evaluations
        [HttpPost("{id}/evaluations")]
        public async Task<IActionResult> AddEvaluation(Guid id, [FromBody] EvaluationCreateDto dto)
        {
            var result = await _pointControleService.AddEvaluationAsync(id, dto);
            return Ok(result);
        }
    }
}
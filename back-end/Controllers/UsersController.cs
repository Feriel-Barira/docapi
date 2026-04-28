using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IDbConnection _connection;

        public UsersController(IDbConnection connection)
        {
            _connection = connection;
        }

        // GET: api/Users/responsables?organisationId={id}
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
    }
}
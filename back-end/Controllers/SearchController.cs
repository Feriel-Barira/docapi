
using DocApi.DTOs;
using DocApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _service;

        public SearchController(ISearchService service)
        {
            _service = service;
        }

        /// <summary>
        /// Recherche multicritère dans toutes les entités :
        /// processus, procédures, documents, non-conformités, indicateurs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string terme,
            [FromQuery] string? organisationId,
            [FromQuery] bool includeProcessus = true,
            [FromQuery] bool includeProcedures = true,
            [FromQuery] bool includeDocuments = true,
            [FromQuery] bool includeNonConformites = true,
            [FromQuery] bool includeIndicateurs = true)
        {
            if (string.IsNullOrWhiteSpace(terme) || terme.Length < 2)
                return BadRequest(new { message = "Le terme doit contenir au moins 2 caractères." });

            var result = await _service.SearchAsync(new SearchRequestDto
            {
                Terme = terme,
                OrganisationId = organisationId,
                IncludeProcessus = includeProcessus,
                IncludeProcedures = includeProcedures,
                IncludeDocuments = includeDocuments,
                IncludeNonConformites = includeNonConformites,
                IncludeIndicateurs = includeIndicateurs
            });

            return Ok(result);
        }
    }
}

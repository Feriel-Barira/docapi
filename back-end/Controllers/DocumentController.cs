using System.Security.Claims;
using DocApi.DTOs;
using DocApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _service;
        private readonly IAuditLogService _auditLog;  

        public DocumentController(IDocumentService service, IAuditLogService auditLog)
        {
            _service = service;
            _auditLog = auditLog;  
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        // ==================== DOCUMENTS ====================

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
        public async Task<IActionResult> GetFiltered([FromQuery] Guid organisationId, [FromQuery] DocumentFilterDto filter)
        {
            var result = await _service.GetFilteredAsync(organisationId, filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Document non trouvé" });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] Guid organisationId, [FromBody] CreateDocumentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.CreateAsync(organisationId, dto);
                await _auditLog.LogAsync("CREATE_DOCUMENT", "Document", result.Id.ToString(), $"Document {result.Code} créé");  // ← AJOUTER
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                await _auditLog.LogAsync("UPDATE_DOCUMENT", "Document", id.ToString()); 
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                await _auditLog.LogAsync("DELETE_DOCUMENT", "Document", id.ToString()); 
                return result ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ==================== VERSIONS ====================

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetVersions(Guid id)
        {
            try
            {
                var result = await _service.GetVersionsAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("{id}/versions")]
        public async Task<IActionResult> AddVersion(Guid id, [FromBody] CreateVersionDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.AddVersionFromDtoAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("versions/{versionId}")]
        public async Task<IActionResult> UpdateVersion(Guid versionId, [FromBody] UpdateVersionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _service.UpdateVersionAsync(versionId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("versions/{versionId}")]
        public async Task<IActionResult> DeleteVersion(Guid versionId)
        {
            try
            {
                var result = await _service.DeleteVersionAsync(versionId);
                return result ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ==================== WORKFLOW VALIDATION ── NOUVEAU ====================

      
        [HttpPost("versions/{versionId}/soumettre")]
        public async Task<IActionResult> Soumettre(Guid versionId)
        {
            try
            {
                var result = await _service.SoumettreVersionAsync(versionId, GetCurrentUserId());
                await _auditLog.LogAsync("UPDATE_DOCUMENT", "Document", versionId.ToString(), "Version soumise pour validation");  // ← AJOUTER
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>EN_REVISION → VALIDE</summary>
        [HttpPost("versions/{versionId}/valider")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Valider(Guid versionId, [FromBody] string? commentaire)
        {
            try
            {
                var result = await _service.ValiderVersionAsync(versionId, GetCurrentUserId(), commentaire);
                await _auditLog.LogAsync("UPDATE_DOCUMENT", "Document", versionId.ToString(), "Version validée");  // ← AJOUTER
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>EN_REVISION → BROUILLON</summary>
        [HttpPost("versions/{versionId}/rejeter")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Rejeter(Guid versionId, [FromBody] string commentaire)
        {
            try
            {
                var result = await _service.RejeterVersionAsync(versionId, GetCurrentUserId(), commentaire);
                await _auditLog.LogAsync("UPDATE_DOCUMENT", "Document", versionId.ToString(), "Version rejetée");  // ← AJOUTER
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>VALIDE → OBSOLETE</summary>
        [HttpPost("versions/{versionId}/archiver")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Archiver(Guid versionId)
        {
            try
            {
                var result = await _service.ArchiverVersionAsync(versionId, GetCurrentUserId());
                await _auditLog.LogAsync("UPDATE_DOCUMENT", "Document", versionId.ToString(), "Version archivée");  // ← AJOUTER
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }
        // ==================== FICHIERS ====================

        /// <summary>
        /// Télécharger un fichier
        /// </summary>
        [HttpGet("download/{versionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(Guid versionId)
        {
            try
            {
                var version = await _service.GetVersionByIdAsync(versionId);
                if (version == null || string.IsNullOrEmpty(version.FichierPath))
                    return NotFound(new { message = "Fichier non trouvé" });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                var filePath = Path.Combine(uploadsFolder, version.FichierPath);

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "Le fichier n'existe pas sur le serveur" });

                var memory = new MemoryStream();
                await using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, GetContentType(filePath), Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Prévisualiser un fichier
        /// </summary>
        [HttpGet("preview/{versionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> PreviewFile(Guid versionId)
        {
            try
            {
                var version = await _service.GetVersionByIdAsync(versionId);
                if (version == null || string.IsNullOrEmpty(version.FichierPath))
                    return NotFound(new { message = "Fichier non trouvé" });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                var filePath = Path.Combine(uploadsFolder, version.FichierPath);

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "Le fichier n'existe pas sur le serveur" });

                var memory = new MemoryStream();
                await using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, GetContentType(filePath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur: {ex.Message}" });
            }
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Aucun fichier fourni" });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Générer un nom unique pour éviter les conflits
                var fileName = file.FileName;
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur: {ex.Message}" });
            }
        }
    }

}

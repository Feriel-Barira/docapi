using DocApi.DTOs;

namespace DocApi.Services.Interfaces
{
    public interface IDocumentService
    {
        // Documents
        Task<DocumentDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<DocumentDto>> GetAllAsync(Guid organisationId, string userRole, int userId);
        Task<IEnumerable<DocumentDto>> GetByProcessusAsync(Guid processusId);
        Task<IEnumerable<DocumentDto>> GetFilteredAsync(Guid organisationId, DocumentFilterDto filter);
        Task<DocumentDto> CreateAsync(Guid organisationId, CreateDocumentDto dto);
        Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto);
        Task<bool> DeleteAsync(Guid id);

        // Versions
        Task<IEnumerable<VersionDocumentDto>> GetVersionsAsync(Guid documentId);
        Task<VersionDocumentDto> AddVersionAsync(Guid documentId, CreateVersionDto dto);
        Task<VersionDocumentDto> AddVersionFromDtoAsync(Guid documentId, CreateVersionDto dto);
        Task<VersionDocumentDto> UpdateVersionAsync(Guid versionId, UpdateVersionDto dto);
        Task<bool> DeleteVersionAsync(Guid versionId);

        // Workflow de validation ── NOUVEAU
        Task<VersionDocumentDto> SoumettreVersionAsync(Guid versionId, int userId);
        Task<VersionDocumentDto> ValiderVersionAsync(Guid versionId, int userId, string? commentaire);
        Task<VersionDocumentDto> RejeterVersionAsync(Guid versionId, int userId, string commentaire);
        Task<VersionDocumentDto> ArchiverVersionAsync(Guid versionId, int userId);
        Task<VersionDocumentDto?> GetVersionByIdAsync(Guid id);
    }
}

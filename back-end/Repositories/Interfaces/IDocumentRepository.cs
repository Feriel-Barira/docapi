// Repositories/Interfaces/IDocumentRepository.cs
using DocApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        // Documents
        Task<Document?> GetByIdAsync(Guid id);
        Task<IEnumerable<Document>> GetAllAsync(Guid organisationId, string userRole, int userId);
        Task<IEnumerable<Document>> GetByProcessusAsync(Guid processusId);
        Task<IEnumerable<Document>> GetFilteredAsync(Guid organisationId, DocumentFilterParams filter);
        Task<Document?> GetByCodeAsync(string code, Guid organisationId);
        Task<Guid> CreateAsync(Document document);
        Task<bool> UpdateAsync(Document document);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null);

        // Versions
        Task<IEnumerable<VersionDocument>> GetVersionsAsync(Guid documentId);
        Task<VersionDocument?> GetVersionByIdAsync(Guid id);
        Task<VersionDocument?> GetLatestVersionAsync(Guid documentId);
        Task<Guid> AddVersionAsync(VersionDocument version);
        Task<bool> UpdateVersionAsync(VersionDocument version);
        Task<bool> DeleteVersionAsync(Guid id);
        Task<bool> SetVersionAsCurrentAsync(Guid documentId, Guid versionId);

        // Compteurs
        Task<int> GetVersionsCountAsync(Guid documentId);
    }

    public class DocumentFilterParams
    {
        public Guid? ProcessusId { get; set; }
        public string? TypeDocument { get; set; }
        public string? SearchTerm { get; set; }
        public bool? Actif { get; set; }
    }
}
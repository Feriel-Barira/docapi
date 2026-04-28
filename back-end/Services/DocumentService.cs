// Services/DocumentService.cs
using DocApi.Domain.Entities;
using DocApi.DTOs;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocApi.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IProcessusRepository _processusRepository;

        public DocumentService(IDocumentRepository repository, IUserRepository userRepository, IProcessusRepository processusRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _processusRepository = processusRepository;
        }

        // ==================== DOCUMENTS ====================

        public async Task<DocumentDto?> GetByIdAsync(Guid id)
        {
            var document = await _repository.GetByIdAsync(id);
            if (document == null) return null;
            return await MapToDto(document);
        }

        public async Task<IEnumerable<DocumentDto>> GetAllAsync(Guid organisationId, string userRole, int userId)
        {
            var documents = await _repository.GetAllAsync(organisationId, userRole, userId);
            var dtos = new List<DocumentDto>();
            foreach (var doc in documents)
                dtos.Add(await MapToDto(doc));
            return dtos;
        }

        public async Task<IEnumerable<DocumentDto>> GetByProcessusAsync(Guid processusId)
        {
            var documents = await _repository.GetByProcessusAsync(processusId);
            var dtos = new List<DocumentDto>();
            foreach (var doc in documents)
                dtos.Add(await MapToDto(doc));
            return dtos;
        }

        public async Task<IEnumerable<DocumentDto>> GetFilteredAsync(Guid organisationId, DocumentFilterDto filter)
        {
            var filterParams = new DocumentFilterParams
            {
                ProcessusId = filter.ProcessusId,
                TypeDocument = filter.TypeDocument,
                SearchTerm = filter.SearchTerm,
                Actif = filter.Statut == "ACTIF" ? true : (filter.Statut == "INACTIF" ? false : (bool?)null)
            };

            var documents = await _repository.GetFilteredAsync(organisationId, filterParams);
            var dtos = new List<DocumentDto>();
            foreach (var doc in documents)
                dtos.Add(await MapToDto(doc));

            if (filter.Page > 0 && filter.PageSize > 0)
                dtos = dtos.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

            return dtos;
        }

        // Services/DocumentService.cs

        public async Task<DocumentDto> CreateAsync(Guid organisationId, CreateDocumentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("Le code du document est requis");
            if (string.IsNullOrWhiteSpace(dto.Titre)) throw new ArgumentException("Le titre du document est requis");
            if (string.IsNullOrWhiteSpace(dto.TypeDocument)) throw new ArgumentException("Le type du document est requis");

            if (await _repository.CodeExistsAsync(dto.Code, organisationId))
                throw new InvalidOperationException($"Un document avec le code {dto.Code} existe déjà");

            if (dto.ProcessusId.HasValue)
            {
                var processus = await _processusRepository.GetByIdAsync(dto.ProcessusId.Value);
                if (processus == null) throw new KeyNotFoundException("Processus non trouvé");
            }

            var document = new Document
            {
                OrganisationId = organisationId,
                Code = dto.Code,
                Titre = dto.Titre,
                TypeDocument = Enum.Parse<DocumentType>(dto.TypeDocument, true),
                Description = dto.Description,
                ProcessusId = dto.ProcessusId,
                Actif = dto.Actif
            };

            var documentId = await _repository.CreateAsync(document);

            // ✅ Création de la version initiale
            if (dto.VersionInitiale != null && !string.IsNullOrWhiteSpace(dto.VersionInitiale.FichierPath))
            {
                var version = new VersionDocument
                {
                    DocumentId = documentId,
                    OrganisationId = organisationId,
                    NumeroVersion = dto.VersionInitiale.NumeroVersion ?? "1.0",
                    Statut = VersionStatut.BROUILLON,
                    FichierPath = dto.VersionInitiale.FichierPath,
                    CommentaireRevision = dto.VersionInitiale.CommentaireRevision,
                    // ✅ dto.VersionInitiale.EtabliParId est déjà un int (non nullable)
                    EtabliParId = dto.VersionInitiale.EtabliParId,  // Pas de ?? nécessaire
                    VerifieParId = dto.VersionInitiale.VerifieParId,
                    ValideParId = dto.VersionInitiale.ValideParId,
                    DateEtablissement = dto.VersionInitiale.DateEtablissement ?? DateTime.UtcNow,
                    DateVerification = dto.VersionInitiale.DateVerification,
                    DateValidation = dto.VersionInitiale.DateValidation,
                    DateMiseEnVigueur = dto.VersionInitiale.DateMiseEnVigueur
                };

                await _repository.AddVersionAsync(version);
            }

            var created = await _repository.GetByIdAsync(documentId);
            if (created == null) throw new Exception("Erreur lors de la création du document");
            return await MapToDto(created);
        }
        public async Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto)
        {
            var document = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Document avec l'ID {id} non trouvé");

            if (!string.IsNullOrWhiteSpace(dto.Code) && dto.Code != document.Code)
            {
                if (await _repository.CodeExistsAsync(dto.Code, document.OrganisationId, id))
                    throw new InvalidOperationException($"Un document avec le code {dto.Code} existe déjà");
                document.Code = dto.Code;
            }

            if (!string.IsNullOrWhiteSpace(dto.Titre)) document.Titre = dto.Titre;
            if (!string.IsNullOrWhiteSpace(dto.TypeDocument)) document.TypeDocument = Enum.Parse<DocumentType>(dto.TypeDocument, true);
            if (dto.Description != null) document.Description = dto.Description;
            if (dto.ProcessusId.HasValue) document.ProcessusId = dto.ProcessusId.Value;
            if (dto.Actif.HasValue) document.Actif = dto.Actif.Value;

            if (!await _repository.UpdateAsync(document))
                throw new Exception("Erreur lors de la mise à jour");

            return await MapToDto(await _repository.GetByIdAsync(id));
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new KeyNotFoundException($"Document avec l'ID {id} non trouvé");
            return await _repository.DeleteAsync(id);
        }

        // ==================== VERSIONS ====================

        public async Task<IEnumerable<VersionDocumentDto>> GetVersionsAsync(Guid documentId)
        {
            var document = await _repository.GetByIdAsync(documentId)
                ?? throw new KeyNotFoundException("Document non trouvé");

            var versions = await _repository.GetVersionsAsync(documentId);
            var dtos = new List<VersionDocumentDto>();
            foreach (var v in versions)
                dtos.Add(await MapVersionToDto(v));
            return dtos.OrderByDescending(v => v.DateEtablissement);
        }

        // Services/DocumentService.cs

        public async Task<VersionDocumentDto> AddVersionFromDtoAsync(Guid documentId, CreateVersionDto dto)
        {
            var document = await _repository.GetByIdAsync(documentId)
                ?? throw new KeyNotFoundException("Document non trouvé");

            var existingVersions = await _repository.GetVersionsAsync(documentId);
            if (existingVersions.Any(v => v.NumeroVersion == dto.NumeroVersion))
                throw new InvalidOperationException($"La version {dto.NumeroVersion} existe déjà");

            var version = new VersionDocument
            {
                DocumentId = documentId,
                OrganisationId = document.OrganisationId,
                NumeroVersion = dto.NumeroVersion,
                Statut = string.IsNullOrEmpty(dto.Statut) ? VersionStatut.BROUILLON : Enum.Parse<VersionStatut>(dto.Statut, true),
                FichierPath = dto.FichierPath ?? "",
                CommentaireRevision = dto.CommentaireRevision,
                // ✅ dto.EtabliParId est int? - valeur par défaut 1 si null
                EtabliParId = dto.EtabliParId ?? 1,
                VerifieParId = dto.VerifieParId,
                ValideParId = dto.ValideParId,
                DateEtablissement = dto.DateEtablissement ?? DateTime.UtcNow,
                DateVerification = dto.DateVerification,
                DateValidation = dto.DateValidation,
                DateMiseEnVigueur = dto.DateMiseEnVigueur
            };

            var versionId = await _repository.AddVersionAsync(version);
            var createdVersion = await _repository.GetVersionByIdAsync(versionId);
            if (createdVersion == null) throw new Exception("Erreur lors de la création de la version");
            return await MapVersionToDto(createdVersion);
        }

        public async Task<VersionDocumentDto> AddVersionAsync(Guid documentId, CreateVersionDto dto)
        {
            return await AddVersionFromDtoAsync(documentId, dto);
        }

        public async Task<VersionDocumentDto> UpdateVersionAsync(Guid versionId, UpdateVersionDto dto)
        {
            var version = await _repository.GetVersionByIdAsync(versionId)
                ?? throw new KeyNotFoundException("Version non trouvée");

            if (!string.IsNullOrWhiteSpace(dto.Statut)) version.Statut = Enum.Parse<VersionStatut>(dto.Statut, true);
            if (!string.IsNullOrWhiteSpace(dto.FichierPath)) version.FichierPath = dto.FichierPath;
            if (dto.CommentaireRevision != null) version.CommentaireRevision = dto.CommentaireRevision;

            if (dto.VerifieParId.HasValue)
            {
                if (await _userRepository.GetByIdAsync(dto.VerifieParId.Value) == null)
                    throw new KeyNotFoundException("Vérificateur non trouvé");
                version.VerifieParId = dto.VerifieParId.Value;
                version.DateVerification = DateTime.UtcNow;
            }

            if (dto.ValideParId.HasValue)
            {
                if (await _userRepository.GetByIdAsync(dto.ValideParId.Value) == null)
                    throw new KeyNotFoundException("Validateur non trouvé");
                version.ValideParId = dto.ValideParId.Value;
                version.DateValidation = DateTime.UtcNow;
                if (version.Statut == VersionStatut.EN_REVISION)
                    version.Statut = VersionStatut.VALIDE;
                version.DateMiseEnVigueur = DateTime.UtcNow;
            }

            if (!await _repository.UpdateVersionAsync(version))
                throw new Exception("Erreur lors de la mise à jour de la version");

            return await MapVersionToDto(await _repository.GetVersionByIdAsync(versionId));
        }

        public async Task<bool> DeleteVersionAsync(Guid versionId)
        {
            var version = await _repository.GetVersionByIdAsync(versionId)
                ?? throw new KeyNotFoundException("Version non trouvée");

            var versions = await _repository.GetVersionsAsync(version.DocumentId);
            if (versions.Count() <= 1)
                throw new InvalidOperationException("Impossible de supprimer la dernière version du document");

            return await _repository.DeleteVersionAsync(versionId);
        }

        // ==================== WORKFLOW VALIDATION ====================

        public async Task<VersionDocumentDto> SoumettreVersionAsync(Guid versionId, int userId)
        {
            var version = await _repository.GetVersionByIdAsync(versionId)
                ?? throw new KeyNotFoundException("Version introuvable.");

            if (version.Statut != VersionStatut.BROUILLON)
                throw new InvalidOperationException(
                    $"Impossible de soumettre : statut actuel '{version.Statut}'. Attendu : BROUILLON.");

            version.Statut = VersionStatut.EN_REVISION;
            version.VerifieParId = userId;
            version.DateVerification = DateTime.UtcNow;

            await _repository.UpdateVersionAsync(version);
            return await MapVersionToDto(await _repository.GetVersionByIdAsync(versionId));
        }

        public async Task<VersionDocumentDto> ValiderVersionAsync(Guid versionId, int userId, string? commentaire)
        {
            var version = await _repository.GetVersionByIdAsync(versionId)
                ?? throw new KeyNotFoundException("Version introuvable.");

            if (version.Statut != VersionStatut.EN_REVISION)
                throw new InvalidOperationException(
                    $"Impossible de valider : statut actuel '{version.Statut}'. Attendu : EN_REVISION.");

            version.Statut = VersionStatut.VALIDE;
            version.ValideParId = userId;
            version.DateValidation = DateTime.UtcNow;
            version.DateMiseEnVigueur = DateTime.UtcNow;
            if (commentaire != null)
                version.CommentaireRevision = commentaire;

            await _repository.UpdateVersionAsync(version);
            return await MapVersionToDto(await _repository.GetVersionByIdAsync(versionId));
        }

        public async Task<VersionDocumentDto> RejeterVersionAsync(Guid versionId, int userId, string commentaire)
        {
            var version = await _repository.GetVersionByIdAsync(versionId)
                ?? throw new KeyNotFoundException("Version introuvable.");

            if (version.Statut != VersionStatut.EN_REVISION)
                throw new InvalidOperationException(
                    $"Impossible de rejeter : statut actuel '{version.Statut}'. Attendu : EN_REVISION.");

            version.Statut = VersionStatut.BROUILLON;
            version.CommentaireRevision = commentaire;

            await _repository.UpdateVersionAsync(version);
            return await MapVersionToDto(await _repository.GetVersionByIdAsync(versionId));
        }

        public async Task<VersionDocumentDto> ArchiverVersionAsync(Guid versionId, int userId)
        {
            var version = await _repository.GetVersionByIdAsync(versionId)
                ?? throw new KeyNotFoundException("Version introuvable.");

            if (version.Statut != VersionStatut.VALIDE)
                throw new InvalidOperationException(
                    $"Impossible d'archiver : statut actuel '{version.Statut}'. Attendu : VALIDE.");

            version.Statut = VersionStatut.OBSOLETE;

            await _repository.UpdateVersionAsync(version);
            return await MapVersionToDto(await _repository.GetVersionByIdAsync(versionId));
        }

        // ==================== MAPPING ====================

        private async Task<DocumentDto> MapToDto(Document document)
        {
            var allVersions = await _repository.GetVersionsAsync(document.Id);
            var versionsCount = allVersions.Count();
            var latestVersion = allVersions.OrderByDescending(v => v.DateEtablissement).FirstOrDefault();
            var processus = document.ProcessusId.HasValue
                ? await _processusRepository.GetByIdAsync(document.ProcessusId.Value)
                : null;

            var versionsDto = new List<VersionDocumentDto>();
            foreach (var v in allVersions.OrderByDescending(v => v.DateEtablissement))
            {
                versionsDto.Add(await MapVersionToDto(v));
            }

            return new DocumentDto
            {
                Id = document.Id,
                Code = document.Code,
                Titre = document.Titre,
                TypeDocument = document.TypeDocument.ToString(),
                Description = document.Description,
                Processus = processus != null ? new DocumentProcessusDto
                {
                    Id = processus.Id,
                    Code = processus.Code,
                    Nom = processus.Nom
                } : null,
                Statut = document.Actif ? "ACTIF" : "INACTIF",
                Actif = document.Actif,
                VersionsCount = versionsCount,
                VersionActuelle = latestVersion != null ? await MapVersionToDto(latestVersion) : null,
                Versions = versionsDto,
                DateCreation = document.DateCreation,
                DateModification = document.DateModification
            };
        }

        private async Task<VersionDocumentDto> MapVersionToDto(VersionDocument version)
        {
            var etabliPar = await _userRepository.GetByIdAsync(version.EtabliParId);
            var verifiePar = version.VerifieParId.HasValue
                ? await _userRepository.GetByIdAsync(version.VerifieParId.Value) : null;
            var validePar = version.ValideParId.HasValue
                ? await _userRepository.GetByIdAsync(version.ValideParId.Value) : null;

            return new VersionDocumentDto
            {
                Id = version.Id,
                NumeroVersion = version.NumeroVersion,
                Statut = version.Statut.ToString(),
                FichierPath = version.FichierPath,
                CommentaireRevision = version.CommentaireRevision,
                EtabliParId = version.EtabliParId,
                VerifieParId = version.VerifieParId,
                ValideParId = version.ValideParId,
                EtabliPar = etabliPar != null ? new VersionAuteurDto
                {
                    Id = etabliPar.Id,
                    NomComplet = $"{etabliPar.Prenom} {etabliPar.Nom}".Trim(),
                    Email = etabliPar.Email
                } : null,
                DateEtablissement = version.DateEtablissement,
                VerifiePar = verifiePar != null ? new VersionAuteurDto
                {
                    Id = verifiePar.Id,
                    NomComplet = $"{verifiePar.Prenom} {verifiePar.Nom}".Trim(),
                    Email = verifiePar.Email
                } : null,
                DateVerification = version.DateVerification,
                ValidePar = validePar != null ? new VersionAuteurDto
                {
                    Id = validePar.Id,
                    NomComplet = $"{validePar.Prenom} {validePar.Nom}".Trim(),
                    Email = validePar.Email
                } : null,
                DateValidation = version.DateValidation,
                DateMiseEnVigueur = version.DateMiseEnVigueur
            };
        }
        public async Task<VersionDocumentDto?> GetVersionByIdAsync(Guid id)
        {
            var version = await _repository.GetVersionByIdAsync(id);
            if (version == null) return null;
            return await MapVersionToDto(version);
        }
    }
}
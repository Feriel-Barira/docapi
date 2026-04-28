// DTOs/DocumentDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace DocApi.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string TypeDocument { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DocumentProcessusDto? Processus { get; set; }
        public string Statut { get; set; } = string.Empty;
        public bool Actif { get; set; }
        public int VersionsCount { get; set; }
        public VersionDocumentDto? VersionActuelle { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public IEnumerable<VersionDocumentDto>? Versions { get; set; }
    }

    public class DocumentProcessusDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
    }

    public class VersionDocumentDto
    {
        public Guid Id { get; set; }
        public string NumeroVersion { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string FichierPath { get; set; } = string.Empty;
        public string? CommentaireRevision { get; set; }

        // ✅ AJOUTER CES PROPRIÉTÉS
        public int? EtabliParId { get; set; }
        public int? VerifieParId { get; set; }
        public int? ValideParId { get; set; }

        public VersionAuteurDto? EtabliPar { get; set; }
        public DateTime DateEtablissement { get; set; }
        public VersionAuteurDto? VerifiePar { get; set; }
        public DateTime? DateVerification { get; set; }
        public VersionAuteurDto? ValidePar { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateMiseEnVigueur { get; set; }
    }

    public class VersionAuteurDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateDocumentDto
    {
        [Required(ErrorMessage = "Le code du document est obligatoire")]
        [MaxLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le titre est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères")]
        public string Titre { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le type de document est obligatoire")]
        public string TypeDocument { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ProcessusId { get; set; }
        public bool Actif { get; set; } = true;
        public CreateVersionDocumentDto VersionInitiale { get; set; } = new();
    }

    public class CreateVersionDocumentDto
    {
        public string NumeroVersion { get; set; } = "1.0";
        public string FichierPath { get; set; } = string.Empty;
        public string? CommentaireRevision { get; set; }
        public int EtabliParId { get; set; } = 1;       
        public int? VerifieParId { get; set; }            
        public int? ValideParId { get; set; }
        public DateTime? DateEtablissement { get; set; }
        public DateTime? DateVerification { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateMiseEnVigueur { get; set; }
    }

    public class UpdateDocumentDto
    {
        [MaxLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string? Code { get; set; }
        [MaxLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères")]
        public string? Titre { get; set; }
        public string? TypeDocument { get; set; }
        public string? Description { get; set; }
        public Guid? ProcessusId { get; set; }
        public bool? Actif { get; set; }
    }

    public class CreateVersionDto
    {
        [Required(ErrorMessage = "Le numéro de version est obligatoire")]
        public string NumeroVersion { get; set; } = string.Empty;
        public string FichierPath { get; set; } = string.Empty;
        public string? CommentaireRevision { get; set; }
        public int? EtabliParId { get; set; }           
        public int? VerifieParId { get; set; }           
        public int? ValideParId { get; set; }
        public DateTime? DateEtablissement { get; set; }
        public DateTime? DateVerification { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateMiseEnVigueur { get; set; }
        public string? Statut { get; set; }
    }

    public class UpdateVersionDto
    {
        public string? Statut { get; set; }
        public string? FichierPath { get; set; }
        public string? CommentaireRevision { get; set; }
        public int? VerifieParId { get; set; }
        public int? ValideParId { get; set; }
    }

    public class DocumentFilterDto
    {
        public Guid? ProcessusId { get; set; }
        public string? TypeDocument { get; set; }
        public string? SearchTerm { get; set; }
        public string? Statut { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
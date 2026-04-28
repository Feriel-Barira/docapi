// Domain/Entities/Document.cs
using System;

namespace DocApi.Domain.Entities
{
    public enum DocumentType
    {
        REFERENCE,
        TRAVAIL
    }

    public enum VersionStatut
    {
        BROUILLON,
        EN_REVISION,
        VALIDE,
        OBSOLETE
    }

    public class Document
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid? ProcessusId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public DocumentType TypeDocument { get; set; }
        public string? Description { get; set; }
        public bool Actif { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public List<VersionDocument> Versions { get; set; } = new();
    }

    public class VersionDocument
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid OrganisationId { get; set; }
        public string NumeroVersion { get; set; } = string.Empty;
        public VersionStatut Statut { get; set; }
        public string FichierPath { get; set; } = string.Empty;
        public string? CommentaireRevision { get; set; }
        public int EtabliParId { get; set; }
        public DateTime DateEtablissement { get; set; }
        public int? VerifieParId { get; set; }
        public DateTime? DateVerification { get; set; }
        public int? ValideParId { get; set; }
        public DateTime? DateValidation { get; set; }
        public DateTime? DateMiseEnVigueur { get; set; }
    }
}
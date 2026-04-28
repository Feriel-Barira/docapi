// Domain/Entities/NonConformite.cs
using System;

namespace DocApi.Domain.Entities
{
    public enum NonConformiteSource
    {
        AUDIT,
        POINT_CONTROLE,
        RECLAMATION,
        AUTRE
    }

    public enum NonConformiteGravite
    {
        MINEURE,
        MAJEURE,
        CRITIQUE
    }

    public enum NonConformiteStatut
    {
        OUVERTE,
        ANALYSE,
        ACTION_EN_COURS,
        CLOTUREE
    }

    public enum AnalyseMethode
    {
        CINQ_M,
        ISHIKAWA,
        CINQ_POURQUOI,
        AUTRE
    }

    public class NonConformite
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid ProcessusId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public DateTime DateDetection { get; set; }
        public int DetecteParId { get; set; }
       
        public string Type { get; set; } = string.Empty;     
        public string Nature { get; set; } = string.Empty;
        public NonConformiteSource Source { get; set; }
        public NonConformiteGravite Gravite { get; set; }
        public NonConformiteStatut Statut { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? ResponsableTraitementId { get; set; }
        public DateTime? DateCloture { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }

    public class AnalyseCauseNonConformite
    {
        public Guid Id { get; set; }
        public Guid NonConformiteId { get; set; }
        public AnalyseMethode MethodeAnalyse { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateAnalyse { get; set; }
        public int AnalyseParId { get; set; }
    }

    public class HistoriqueNonConformite
    {
        public Guid Id { get; set; }
        public Guid NonConformiteId { get; set; }
        public NonConformiteStatut AncienStatut { get; set; }
        public NonConformiteStatut NouveauStatut { get; set; }
        public DateTime DateChangement { get; set; }
        public int ChangeParId { get; set; }
        public string? Commentaire { get; set; }
    }
}
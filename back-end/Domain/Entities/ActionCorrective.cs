// Domain/Entities/ActionCorrective.cs
using System;

namespace DocApi.Domain.Entities
{
    public enum ActionCorrectiveType
    {
        CURATIVE,
        CORRECTIVE,
        PREVENTIVE
    }

    public enum ActionCorrectiveStatut
    {
        PLANIFIEE,
        EN_COURS,
        REALISEE,
        VERIFIEE
    }

    public class ActionCorrective
    {
        public Guid Id { get; set; }
        public Guid NonConformiteId { get; set; }
        public ActionCorrectiveType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public int ResponsableId { get; set; }
        public DateTime DateEcheance { get; set; }
        public ActionCorrectiveStatut Statut { get; set; }
        public DateTime? DateRealisation { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public Guid? PreuveEnregistrementId { get; set; }
    }
}
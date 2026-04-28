// Domain/Entities/Procedure.cs
using System;

namespace DocApi.Domain.Entities
{
    public enum ProcedureStatut
    {
        ACTIF,
        INACTIF
    }

    public class Procedure
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid ProcessusId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string? Objectif { get; set; }
        public string? DomaineApplication { get; set; }
        public string? Description { get; set; }
        public int ResponsableId { get; set; }
        public ProcedureStatut Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public string? ProcessusCode { get; set; }
        public string? ProcessusNom { get; set; }
    }

    public class Instruction
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid ProcedureId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Ordre { get; set; }
        public ProcedureStatut Statut { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
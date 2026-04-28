using System;

namespace DocApi.Domain
{
    public class Enregistrement
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid ProcessusId { get; set; }
        public string TypeEnregistrement { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FichierPath { get; set; } = string.Empty;
        public DateTime DateEnregistrement { get; set; }
        public int CreeParId { get; set; }
        public string? ProcessusCode { get; set; }

        // Pour jointures (non mappés directement)
        public string? ProcessusNom { get; set; }
        public string? CreeParNom { get; set; }
    }
}
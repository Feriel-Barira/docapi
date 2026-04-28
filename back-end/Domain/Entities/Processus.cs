using System;

namespace DocApi.Domain.Entities
{
    public enum ProcessusType
    {
        PILOTAGE,
        REALISATION,
        SUPPORT
    }

    public enum ProcessusStatut
    {
        ACTIF,
        INACTIF
    }

    public enum TypeActeur
    {
        PILOTE,
        COPILOTE,
        CONTRIBUTEUR,
        OBSERVATEUR
    }

    public class Processus
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProcessusType Type { get; set; }
        public string Finalites { get; set; } = "[]";
        public string Perimetres { get; set; } = "[]";
        public string Fournisseurs { get; set; } = "[]";
        public string Clients { get; set; } = "[]";
        public string DonneesEntree { get; set; } = "[]";
        public string DonneesSortie { get; set; } = "[]";
        public string Objectifs { get; set; } = "[]";
        public int PiloteId { get; set; }
        public ProcessusStatut Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }

    public class ProcessusActeur
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid ProcessusId { get; set; }
        public int UtilisateurId { get; set; }
        public TypeActeur TypeActeur { get; set; }
        public DateTime DateAffectation { get; set; }
    }
}
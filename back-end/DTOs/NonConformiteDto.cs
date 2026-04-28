// DTOs/NonConformiteDto.cs
using System;
using System.Collections.Generic;

namespace DocApi.DTOs
{
    public class NonConformiteDto
    {
        public Guid Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateDetection { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Gravite { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;

        // ✅ AJOUTER CES DEUX PROPRIÉTÉS
        public string Type { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;

        public NonConformiteProcessusDto? Processus { get; set; }
        public NonConformiteUtilisateurDto? DetectePar { get; set; }
        public NonConformiteUtilisateurDto? ResponsableTraitement { get; set; }
        public AnalyseCauseDto? AnalyseCause { get; set; }
        public List<ActionCorrectiveDto> ActionsCorrectives { get; set; } = new();
        public List<HistoriqueNonConformiteDto> Historique { get; set; } = new();
        public DateTime? DateCloture { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }

    public class NonConformiteProcessusDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
    }

    public class NonConformiteUtilisateurDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
    }

    public class AnalyseCauseDto
    {
        public Guid Id { get; set; }
        public string MethodeAnalyse { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateAnalyse { get; set; }
        public NonConformiteUtilisateurDto? AnalysePar { get; set; }
    }


    public class HistoriqueNonConformiteDto
    {
        public Guid Id { get; set; }
        public string AncienStatut { get; set; } = string.Empty;
        public string NouveauStatut { get; set; } = string.Empty;
        public DateTime DateChangement { get; set; }
        public NonConformiteUtilisateurDto? ChangePar { get; set; }
        public string? Commentaire { get; set; }
    }

    public class CreateNonConformiteDto
    {
        public Guid ProcessusId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Gravite { get; set; } = string.Empty;
        public int? ResponsableTraitementId { get; set; }
        public int DetecteParId { get; set; }
        public DateTime DateDetection { get; set; }
    }

    public class UpdateNonConformiteDto
    {
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Nature { get; set; }
        public string? Source { get; set; }
        public string? Gravite { get; set; }
        public Guid? ProcessusId { get; set; }
        public int? ResponsableTraitementId { get; set; }
        public int? DetecteParId { get; set; }
        public DateTime? DateDetection { get; set; }
    }

    public class CreateAnalyseCauseDto
    {
        public string MethodeAnalyse { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class NonConformiteFilterDto
    {
        public Guid? ProcessusId { get; set; }
        public string? Source { get; set; }
        public string? Gravite { get; set; }
        public string? Statut { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace DocApi.DTOs
{
    public class ProcessusDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string Type { get; set; } = string.Empty;
        public List<string> Finalites { get; set; } = new();
        public List<string> Perimetres { get; set; } = new();
        public List<string> Fournisseurs { get; set; } = new();
        public List<string> Clients { get; set; } = new();
        public List<string> DonneesEntree { get; set; } = new();
        public List<string> DonneesSortie { get; set; } = new();
        public List<string> Objectifs { get; set; } = new();

        public ProcessusPiloteDto? Pilote { get; set; }
        public string Statut { get; set; } = string.Empty;
        public int ProceduresCount { get; set; }
        public int DocumentsCount { get; set; }
        public int ActeursCount { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }

    public class ProcessusPiloteDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
    }

    public class ProcessusActeurDto
    {
        public Guid Id { get; set; }
        public int UtilisateurId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TypeActeur { get; set; } = string.Empty;
        public DateTime DateAffectation { get; set; }
    }

    public class CreateProcessusDto
    {
        [Required(ErrorMessage = "Le code est obligatoire")]
        [MaxLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required(ErrorMessage = "Le type est obligatoire")]
        public string Type { get; set; } = string.Empty;
        public List<string> Finalites { get; set; } = new();
        public List<string> Perimetres { get; set; } = new();
        public List<string> Fournisseurs { get; set; } = new();
        public List<string> Clients { get; set; } = new();
        public List<string> DonneesEntree { get; set; } = new();
        public List<string> DonneesSortie { get; set; } = new();
        public List<string> Objectifs { get; set; } = new();
        [Required(ErrorMessage = "Le pilote est obligatoire")]
        public int PiloteId { get; set; }
        [Required(ErrorMessage = "Le statut est obligatoire")]

        public string Statut { get; set; } = "ACTIF";
        public List<CreateProcessusActeurDto> Acteurs { get; set; } = new();
    }

    public class UpdateProcessusDto
    {
        [MaxLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string? Code { get; set; }
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string? Nom { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public List<string>? Finalites { get; set; }
        public List<string>? Perimetres { get; set; }
        public List<string>? Fournisseurs { get; set; }
        public List<string>? Clients { get; set; }
        public List<string>? DonneesEntree { get; set; }
        public List<string>? DonneesSortie { get; set; }
        public List<string>? Objectifs { get; set; }
        public int? PiloteId { get; set; }
        public string? Statut { get; set; }
        public List<CreateProcessusActeurDto>? Acteurs { get; set; }  
    }

    public class CreateProcessusActeurDto
    {
        public int UtilisateurId { get; set; }
        public string TypeActeur { get; set; } = string.Empty;
    }

    public class ProcessusFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Type { get; set; }
        public string? Statut { get; set; }
        public int? PiloteId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
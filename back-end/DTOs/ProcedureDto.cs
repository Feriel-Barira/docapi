// DTOs/ProcedureDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace DocApi.DTOs
{
    public class ProcedureDto
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }  
        public Guid ProcessusId { get; set; } 
        public int ResponsableId { get; set; }  
        public string Code { get; set; } = string.Empty;
        public string? ProcessusCode { get; set; } 
        public string? ProcessusNom { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Objectif { get; set; }
        public string? DomaineApplication { get; set; }
        public string? Description { get; set; }
        public ProcedureResponsableDto? Responsable { get; set; }
        public string Statut { get; set; } = string.Empty;
        public int InstructionsCount { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public List<InstructionDto> Instructions { get; set; } = new();
    }


    public class ProcedureResponsableDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
    }

    public class InstructionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Ordre { get; set; }
        public string Statut { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
    }

    public class CreateProcedureDto
    {
        [Required(ErrorMessage = "Le code est obligatoire")]
        [MaxLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le titre est obligatoire")]
        [MaxLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères")]
        public string Titre { get; set; } = string.Empty;
        public string? Objectif { get; set; }
        public string? DomaineApplication { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Le responsable est obligatoire")]
        public int ResponsableId { get; set; }
        public string Statut { get; set; } = "ACTIF";
        public List<CreateInstructionDto> Instructions { get; set; } = new();
    }

    public class UpdateProcedureDto
    {
        [MaxLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string? Code { get; set; }
        [MaxLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères")]
        public string? Titre { get; set; }
        public string? Objectif { get; set; }
        public string? DomaineApplication { get; set; }
        public string? Description { get; set; }
        public int? ResponsableId { get; set; }
        public string? Statut { get; set; }
        public List<CreateInstructionDto>? Instructions { get; set; }
    }

    public class CreateInstructionDto
    {
        [Required(ErrorMessage = "Le code de l'instruction est obligatoire")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le titre de l'instruction est obligatoire")]
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Ordre { get; set; }
        public string Statut { get; set; } = "ACTIF";
    }

    public class UpdateInstructionDto
    {
        public string? Code { get; set; }
        public string? Titre { get; set; }
        public string? Description { get; set; }
        public int? Ordre { get; set; }
        public string? Statut { get; set; }
    }

    public class ProcedureFilterDto
    {
        public Guid? ProcessusId { get; set; }
        public string? SearchTerm { get; set; }
        public string? Statut { get; set; }
        public int? ResponsableId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
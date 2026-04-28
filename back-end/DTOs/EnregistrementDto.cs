using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DocApi.DTOs
{
    
    public class EnregistrementDto
    {
        public Guid Id { get; set; }
        public Guid ProcessusId { get; set; }
        public string ProcessusNom { get; set; } = string.Empty;
        public string ProcessusCode { get; set; } = string.Empty;
        public string TypeEnregistrement { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FichierPath { get; set; } = string.Empty;
        public DateTime DateEnregistrement { get; set; }
        public string CreeParNom { get; set; } = string.Empty;
    }

    // DTO pour la création (upload de fichier)
    public class CreateEnregistrementDto
    {
        [Required(ErrorMessage = "Le processus est obligatoire")]
        public Guid ProcessusId { get; set; }
        [Required]
        public Guid OrganisationId { get; set; }  

        [MaxLength(50, ErrorMessage = "Le type ne peut pas dépasser 50 caractères")]
        public string TypeEnregistrement { get; set; } = "PREUVE_EXECUTION";

        [MaxLength(100, ErrorMessage = "La référence ne peut pas dépasser 100 caractères")]
        public string Reference { get; set; } = "";

        [MaxLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le fichier est obligatoire")]
        public IFormFile Fichier { get; set; } = null!;
    }

    // DTO pour la mise à jour (si nécessaire)
    public class UpdateEnregistrementDto
    {
        [MaxLength(50)]
        public string? TypeEnregistrement { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    // DTO pour filtrer la liste
    public class EnregistrementFilterDto
    {
        public Guid? ProcessusId { get; set; }
        public string? TypeEnregistrement { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
// DTOs/ActionCorrectiveDto.cs
using System;
using System.Collections.Generic;

namespace DocApi.DTOs
{
    public class ActionCorrectiveDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ActionCorrectiveResponsableDto? Responsable { get; set; }
        public DateTime DateEcheance { get; set; }
        public string Statut { get; set; } = string.Empty;
        public DateTime? DateRealisation { get; set; }
       
        public DateTime DateCreation { get; set; }
        public Guid? PreuveEnregistrementId { get; set; }
    }

    public class ActionCorrectiveResponsableDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateActionCorrectiveDto
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ResponsableId { get; set; }
        public DateTime DateEcheance { get; set; }
    }

    public class UpdateActionCorrectiveDto
    {
        public string? Description { get; set; }
        public int? ResponsableId { get; set; }
        public DateTime? DateEcheance { get; set; }
        public string? Statut { get; set; }
      
        public DateTime? DateRealisation { get; set; }
    }

    public class VerifierActionCorrectiveDto
    {
        public bool Efficace { get; set; }
        public string? Commentaire { get; set; }
    }
}
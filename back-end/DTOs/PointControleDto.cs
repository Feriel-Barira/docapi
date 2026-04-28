using System;
using System.Collections.Generic;

namespace DocApi.DTOs
{
    // ==================== CREATE DTOs ====================
    public class PointControleCreateDto
    {
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ProcessusId { get; set; }
        public string Type { get; set; } = "DOCUMENTAIRE";
        public string Frequence { get; set; } = "ANNUEL";
        public int? ResponsableId { get; set; }
        public bool Actif { get; set; } = true;
    }

    public class EvaluationCreateDto
    {
        public DateTime DateEvaluation { get; set; }
        public bool Conforme { get; set; }
        public string? Commentaire { get; set; }
        public int? EvalueParId { get; set; }
    }

    // ==================== RESPONSE DTOs ====================
    public class EvaluationResponseDto
    {
        public Guid Id { get; set; }
        public Guid PointControleId { get; set; }
        public DateTime DateEvaluation { get; set; }
        public bool Conforme { get; set; }
        public string? Commentaire { get; set; }
        public int? EvalueParId { get; set; }
        public string? EvalueParNom { get; set; }
        public DateTime DateCreation { get; set; }
    }

    public class PointControleResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid? ProcessusId { get; set; }
        public string? ProcessusCode { get; set; }
        public string? ProcessusNom { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Frequence { get; set; } = string.Empty;
        public int? ResponsableId { get; set; }
        public string? ResponsableNom { get; set; }
        public bool Actif { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public EvaluationResponseDto? DerniereEvaluation { get; set; }
        public List<EvaluationResponseDto> Evaluations { get; set; } = new();
    }
}
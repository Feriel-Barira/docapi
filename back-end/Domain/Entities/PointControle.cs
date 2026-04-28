using System;

namespace DocApi.Domain
{
    // ==================== POINT DE CONTROLE ====================
    public class PointControle
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid? ProcessusId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "DOCUMENTAIRE";
        public string Frequence { get; set; } = "ANNUEL";
        public int? ResponsableId { get; set; }
        public bool Actif { get; set; } = true;
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }

    // ==================== EVALUATION POINT DE CONTROLE ====================
    public class EvaluationPointControle
    {
        public Guid Id { get; set; }
        public Guid PointControleId { get; set; }
        public DateTime DateEvaluation { get; set; }
        public bool Conforme { get; set; }
        public string? Commentaire { get; set; }
        public int? EvalueParId { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
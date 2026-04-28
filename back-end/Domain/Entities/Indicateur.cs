// Domain/Entities/Indicateur.cs
using System;
using System.Collections.Generic;

namespace DocApi.Domain.Entities
{
    public enum FrequenceMesure
    {
        QUOTIDIEN,
        HEBDOMADAIRE,
        MENSUEL,
        TRIMESTRIEL,
        ANNUEL
    }

    public class Indicateur
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid ProcessusId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MethodeCalcul { get; set; }
        public string? Unite { get; set; }
        public decimal? ValeurCible { get; set; }
        public decimal? SeuilAlerte { get; set; }
        public FrequenceMesure FrequenceMesure { get; set; }
        public int ResponsableId { get; set; }
        public bool Actif { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }

    public class IndicateurValeur
    {
        public Guid Id { get; set; }
        public Guid IndicateurId { get; set; }
        public string Periode { get; set; } = string.Empty;
        public decimal Valeur { get; set; }
        public string? Commentaire { get; set; }
        public DateTime DateMesure { get; set; }
        public int SaisiParId { get; set; }
        public Guid OrganisationId { get; set; }
    }
}
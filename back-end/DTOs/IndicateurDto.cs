// DTOs/IndicateurDto.cs
using System;
using System.Collections.Generic;

namespace DocApi.DTOs
{
    public class IndicateurDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MethodeCalcul { get; set; }
        public string? Unite { get; set; }
        public decimal? ValeurCible { get; set; }
        public decimal? SeuilAlerte { get; set; }
        public string FrequenceMesure { get; set; } = string.Empty;
        public IndicateurResponsableDto? Responsable { get; set; }
        public IndicateurProcessusDto? Processus { get; set; }
        public string Statut { get; set; } = string.Empty;
        public decimal? DerniereValeur { get; set; }
        public string? DernierePeriode { get; set; }
        public decimal? Tendance { get; set; }
        public List<IndicateurValeurDto> Valeurs { get; set; } = new();
        public DateTime DateCreation { get; set; }
        public bool Actif { get; set; }
    }

    public class IndicateurResponsableDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class IndicateurProcessusDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
    }

    public class IndicateurValeurDto
    {
        public Guid Id { get; set; }
        public string Periode { get; set; } = string.Empty;
        public decimal Valeur { get; set; }
        public string? Commentaire { get; set; }
        public DateTime DateMesure { get; set; }
        public IndicateurResponsableDto? SaisiPar { get; set; }
    }

    public class CreateIndicateurDto
    {
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MethodeCalcul { get; set; }
        public string? Unite { get; set; }
        public decimal? ValeurCible { get; set; }
        public decimal? SeuilAlerte { get; set; }
        public string FrequenceMesure { get; set; } = string.Empty;
        public int ResponsableId { get; set; }
        public bool Actif { get; set; } = true;
        public Guid? ProcessusId { get; set; }
    }

    public class UpdateIndicateurDto
    {
        public string? Code { get; set; }
        public string? Nom { get; set; }
        public string? Description { get; set; }
        public string? MethodeCalcul { get; set; }
        public string? Unite { get; set; }
        public decimal? ValeurCible { get; set; }
        public decimal? SeuilAlerte { get; set; }
        public string? FrequenceMesure { get; set; }
        public int? ResponsableId { get; set; }
        public bool? Actif { get; set; }
        public Guid? ProcessusId { get; set; }
    }

    public class CreateIndicateurValeurDto
    {
        public string Periode { get; set; } = string.Empty;
        public decimal Valeur { get; set; }
        public string? Commentaire { get; set; }
        public string? DateMesure { get; set; }  
        public int SaisiParId { get; set; }
    }

    public class IndicateurFilterDto
    {
        public Guid? ProcessusId { get; set; }
        public string? SearchTerm { get; set; }
        public string? Statut { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class UpdateStatutRequest
    {
        public bool Actif { get; set; }
    }
}
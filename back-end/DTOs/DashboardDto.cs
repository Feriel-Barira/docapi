namespace DocApi.DTOs
{
    public class DashboardDto
    {
        public StatistiquesGeneralesDto Statistiques { get; set; } = new();
        public List<AlerteDto> Alertes { get; set; } = new();
        public List<ActiviteRecenteDto> ActivitesRecentes { get; set; } = new();
        public RepartitionNcDto RepartitionNonConformites { get; set; } = new();
        public List<IndicateurResumDto> IndicateursHorsCible { get; set; } = new();
    }

    public class StatistiquesGeneralesDto
    {
        public int TotalProcessus { get; set; }
        public int ProcessusActifs { get; set; }
        public int TotalProcedures { get; set; }
        public int ProceduresActives { get; set; }
        public int TotalDocuments { get; set; }
        public int DocumentsValides { get; set; }
        public int DocumentsEnAttente { get; set; }
        public int TotalNonConformites { get; set; }
        public int NcOuvertes { get; set; }
        public int NcAnalyse { get; set; }
        public int NcActionEnCours { get; set; }
        public int NcCloturees { get; set; }
        public int TotalActionsCorrectives { get; set; }
        public int ActionsEnRetard { get; set; }
        public int ActionsEcheanceProche { get; set; }
        public int TotalIndicateurs { get; set; }
        public int IndicateursActifs { get; set; }
    }

    public class AlerteDto
    {
        public string Type { get; set; } = "";       
        public string Niveau { get; set; } = "";    
        public string Message { get; set; } = "";
        public string? EntityId { get; set; }
        public string? EntityType { get; set; }
        public DateTime Date { get; set; }
    }

    public class ActiviteRecenteDto
    {
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public string EntityNom { get; set; } = "";
        public string Username { get; set; } = "";
        public DateTime Date { get; set; }
    }

    public class RepartitionNcDto
    {
        public int Mineure { get; set; }
        public int Majeure { get; set; }
        public int Critique { get; set; }
        public int ParAudit { get; set; }
        public int ParPointControle { get; set; }
        public int ParReclamation { get; set; }
        public int ParAutre { get; set; }
    }

    public class IndicateurResumDto
    {
        public string Id { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal? ValeurCible { get; set; }
        public decimal? DerniereValeur { get; set; }
        public string? Periode { get; set; }
        public string Unite { get; set; } = "";
    }
}
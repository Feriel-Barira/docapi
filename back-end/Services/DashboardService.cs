using DocApi.DTOs;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;

namespace DocApi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepo;
        private readonly IAuditLogRepository _auditRepo;

        public DashboardService(IDashboardRepository dashboardRepo, IAuditLogRepository auditRepo)
        {
            _dashboardRepo = dashboardRepo;
            _auditRepo = auditRepo;
        }

        public async Task<DashboardDto> GetDashboardAsync(Guid organisationId)
        {
            // Toutes les requêtes en parallèle
            var statsTask = _dashboardRepo.GetStatistiquesAsync(organisationId);
            var repartitionTask = _dashboardRepo.GetRepartitionNcAsync(organisationId);
            var indicateursTask = _dashboardRepo.GetIndicateursHorsCibleAsync(organisationId);
            var activitesTask = _auditRepo.GetRecentAsync(10);

            await Task.WhenAll(statsTask, repartitionTask, indicateursTask, activitesTask);

            var stats = statsTask.Result;
            var repartition = repartitionTask.Result;
            var indicateurs = indicateursTask.Result;
            var activites = activitesTask.Result;

            return new DashboardDto
            {
                Statistiques = stats,
                Alertes = BuildAlertes(stats),
                RepartitionNonConformites = repartition,
                IndicateursHorsCible = indicateurs,
                ActivitesRecentes = activites.Select(a => new ActiviteRecenteDto
                {
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityNom = a.EntityId ?? "",
                    Username = a.Username,
                    Date = a.DateAction
                }).ToList()
            };
        }

        private static List<AlerteDto> BuildAlertes(StatistiquesGeneralesDto stats)
        {
            var alertes = new List<AlerteDto>();

            if (stats.ActionsEnRetard > 0)
                alertes.Add(new AlerteDto
                {
                    Type = "RETARD",
                    Niveau = "DANGER",
                    Message = $"{stats.ActionsEnRetard} action(s) corrective(s) en retard.",
                    EntityType = "ActionCorrective",
                    Date = DateTime.UtcNow
                });

            if (stats.ActionsEcheanceProche > 0)
                alertes.Add(new AlerteDto
                {
                    Type = "ECHEANCE",
                    Niveau = "WARNING",
                    Message = $"{stats.ActionsEcheanceProche} action(s) corrective(s) à échéance dans 7 jours.",
                    EntityType = "ActionCorrective",
                    Date = DateTime.UtcNow
                });

            if (stats.NcOuvertes > 0)
                alertes.Add(new AlerteDto
                {
                    Type = "NC_OUVERTE",
                    Niveau = "WARNING",
                    Message = $"{stats.NcOuvertes} non-conformité(s) ouverte(s) non traitée(s).",
                    EntityType = "NonConformite",
                    Date = DateTime.UtcNow
                });

            if (stats.DocumentsEnAttente > 0)
                alertes.Add(new AlerteDto
                {
                    Type = "DOC_ATTENTE",
                    Niveau = "INFO",
                    Message = $"{stats.DocumentsEnAttente} document(s) en attente de validation.",
                    EntityType = "Document",
                    Date = DateTime.UtcNow
                });

            return alertes;
        }
    }
}

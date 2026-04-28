using DocApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Repositories.Interfaces
{
    public interface IProcessusRepository
    {
        // Récupérer tous les processus (Filtré par Org pour User, Tout pour Admin)
        Task<IEnumerable<Processus>> GetAllAsync(
     Guid organisationId, string userRole, int userId);

        // Récupérer un processus spécifique par son ID
        Task<Processus?> GetByIdAsync(Guid id);

        // Recherche filtrée (Filtrée par Org pour User, Tout pour Admin)
        Task<IEnumerable<Processus>> GetFilteredAsync(
     Guid organisationId, string userRole, int userId,
     string? searchTerm, string? type, string? statut, int? piloteId);

        // Vérifier si un code existe déjà dans une organisation
        Task<Processus?> GetByCodeAsync(string code, Guid organisationId);

        // Opérations de Persistance
        Task<Guid> CreateAsync(Processus processus);
        Task<bool> UpdateAsync(Processus processus);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        // Validation de doublon de code
        Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null);

        // Statistiques et Comptage
        Task<int> GetProceduresCountAsync(Guid processusId);
        Task<int> GetDocumentsCountAsync(Guid processusId);

        // Gestion des Acteurs du processus
        Task<IEnumerable<ProcessusActeur>> GetActeursAsync(Guid processusId);
        Task<Guid> AddActeurAsync(ProcessusActeur acteur);
        Task<bool> RemoveActeurAsync(Guid acteurId);
    }
}
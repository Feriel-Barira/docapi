using DocApi.Domain.Entities;
using DocApi.DTOs;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DocApi.Services
{
    public class ProcessusService : IProcessusService
    {
        private readonly IProcessusRepository _repository;
        private readonly IUserRepository _userRepository;

        public ProcessusService(IProcessusRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        // ✅ CORRIGÉ : Ajout de userId
        public async Task<IEnumerable<ProcessusDto>> GetAllAsync(Guid organisationId, string userRole, int userId)
        {
            var processus = await _repository.GetAllAsync(organisationId, userRole, userId);

            var dtos = new List<ProcessusDto>();
            foreach (var p in processus)
            {
                dtos.Add(await MapToDto(p));
            }

            return dtos;
        }

        public async Task<ProcessusDto?> GetByIdAsync(Guid id)
        {
            var processus = await _repository.GetByIdAsync(id);
            if (processus == null)
                return null;

            return await MapToDto(processus);
        }

        // ✅ CORRIGÉ : Ajout de userId
        public async Task<IEnumerable<ProcessusDto>> GetFilteredAsync(Guid organisationId, string userRole, int userId, ProcessusFilterDto filter)
        {
            var processus = await _repository.GetFilteredAsync(
                organisationId,
                userRole,
                userId,
                filter.SearchTerm,
                filter.Type,
                filter.Statut,
                filter.PiloteId);

            var dtos = new List<ProcessusDto>();
            foreach (var p in processus)
            {
                dtos.Add(await MapToDto(p));
            }

            if (filter.Page > 0 && filter.PageSize > 0)
            {
                dtos = dtos.Skip((filter.Page - 1) * filter.PageSize)
                           .Take(filter.PageSize)
                           .ToList();
            }

            return dtos;
        }

        public async Task<ProcessusDto> CreateAsync(Guid organisationId, CreateProcessusDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Code))
                throw new ArgumentException("Le code du processus est requis");

            if (string.IsNullOrWhiteSpace(createDto.Nom))
                throw new ArgumentException("Le nom du processus est requis");

            if (string.IsNullOrWhiteSpace(createDto.Type))
                throw new ArgumentException("Le type du processus est requis");

            if (createDto.PiloteId <= 0)
                throw new ArgumentException("Le pilote du processus est requis");

            if (!Enum.TryParse<ProcessusType>(createDto.Type, true, out var type))
                throw new ArgumentException("Le type doit être PILOTAGE, REALISATION ou SUPPORT.");

            var codeExists = await _repository.CodeExistsAsync(createDto.Code, organisationId);
            if (codeExists)
                throw new InvalidOperationException($"Un processus avec le code {createDto.Code} existe déjà");

            var pilote = await _userRepository.GetByIdAsync(createDto.PiloteId);
            if (pilote == null)
                throw new KeyNotFoundException("Pilote non trouvé");

            var processus = new Processus
            {
                OrganisationId = organisationId,
                Code = createDto.Code,
                Nom = createDto.Nom,
                Description = createDto.Description,
                Type = type,
                Finalites = JsonSerializer.Serialize(createDto.Finalites ?? new List<string>()),
                Perimetres = JsonSerializer.Serialize(createDto.Perimetres ?? new List<string>()),
                Fournisseurs = JsonSerializer.Serialize(createDto.Fournisseurs ?? new List<string>()),
                Clients = JsonSerializer.Serialize(createDto.Clients ?? new List<string>()),
                DonneesEntree = JsonSerializer.Serialize(createDto.DonneesEntree ?? new List<string>()),
                DonneesSortie = JsonSerializer.Serialize(createDto.DonneesSortie ?? new List<string>()),
                Objectifs = JsonSerializer.Serialize(createDto.Objectifs ?? new List<string>()),
                PiloteId = createDto.PiloteId,
                Statut = Enum.Parse<ProcessusStatut>(createDto.Statut ?? "ACTIF", true)
            };

            var processusId = await _repository.CreateAsync(processus);
            var createdProcessus = await _repository.GetByIdAsync(processusId);

            if (createDto.Acteurs != null && createDto.Acteurs.Any())
            {
                foreach (var acteurDto in createDto.Acteurs)
                {
                    if (acteurDto.UtilisateurId > 0 && !string.IsNullOrWhiteSpace(acteurDto.TypeActeur))
                    {
                        var acteur = new ProcessusActeur
                        {
                            OrganisationId = organisationId,
                            ProcessusId = processusId,
                            UtilisateurId = acteurDto.UtilisateurId,
                            TypeActeur = Enum.Parse<TypeActeur>(acteurDto.TypeActeur, true)
                        };
                        await _repository.AddActeurAsync(acteur);
                    }
                }
            }

            return await MapToDto(createdProcessus!);
        }

        public async Task<ProcessusDto> UpdateAsync(Guid id, UpdateProcessusDto updateDto)
        {
            var processus = await _repository.GetByIdAsync(id);
            if (processus == null)
                throw new KeyNotFoundException($"Processus avec l'ID {id} non trouvé");

            if (!string.IsNullOrWhiteSpace(updateDto.Code) && updateDto.Code != processus.Code)
            {
                var codeExists = await _repository.CodeExistsAsync(updateDto.Code, processus.OrganisationId, id);
                if (codeExists)
                    throw new InvalidOperationException($"Un processus avec le code {updateDto.Code} existe déjà");
                processus.Code = updateDto.Code;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Nom))
                processus.Nom = updateDto.Nom;

            if (updateDto.Description != null)
                processus.Description = updateDto.Description;

            if (!string.IsNullOrWhiteSpace(updateDto.Type))
            {
                if (!Enum.TryParse<ProcessusType>(updateDto.Type, true, out var type))
                    throw new ArgumentException("Le type doit être PILOTAGE, REALISATION ou SUPPORT.");
                processus.Type = type;
            }

            if (updateDto.Finalites != null)
                processus.Finalites = JsonSerializer.Serialize(updateDto.Finalites);
            if (updateDto.Perimetres != null)
                processus.Perimetres = JsonSerializer.Serialize(updateDto.Perimetres);
            if (updateDto.Fournisseurs != null)
                processus.Fournisseurs = JsonSerializer.Serialize(updateDto.Fournisseurs);
            if (updateDto.Clients != null)
                processus.Clients = JsonSerializer.Serialize(updateDto.Clients);
            if (updateDto.DonneesEntree != null)
                processus.DonneesEntree = JsonSerializer.Serialize(updateDto.DonneesEntree);
            if (updateDto.DonneesSortie != null)
                processus.DonneesSortie = JsonSerializer.Serialize(updateDto.DonneesSortie);
            if (updateDto.Objectifs != null)
                processus.Objectifs = JsonSerializer.Serialize(updateDto.Objectifs);

            if (updateDto.PiloteId.HasValue && updateDto.PiloteId.Value > 0)
            {
                var pilote = await _userRepository.GetByIdAsync(updateDto.PiloteId.Value);
                if (pilote == null)
                    throw new KeyNotFoundException("Pilote non trouvé");
                processus.PiloteId = updateDto.PiloteId.Value;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Statut))
                processus.Statut = Enum.Parse<ProcessusStatut>(updateDto.Statut, true);

            var updated = await _repository.UpdateAsync(processus);
            if (!updated)
                throw new Exception("Erreur lors de la mise à jour du processus");

            var updatedProcessus = await _repository.GetByIdAsync(id);
            return await MapToDto(updatedProcessus!);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new KeyNotFoundException($"Processus avec l'ID {id} non trouvé");

            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProcessusActeurDto>> GetActeursAsync(Guid processusId)
        {
            var acteurs = await _repository.GetActeursAsync(processusId);
            var dtos = new List<ProcessusActeurDto>();

            foreach (var acteur in acteurs)
            {
                var utilisateur = await _userRepository.GetByIdAsync(acteur.UtilisateurId);
                dtos.Add(new ProcessusActeurDto
                {
                    Id = acteur.Id,
                    UtilisateurId = acteur.UtilisateurId,
                    NomComplet = utilisateur != null ? utilisateur.Username : string.Empty,
                    Email = utilisateur?.Email ?? string.Empty,
                    TypeActeur = acteur.TypeActeur.ToString(),
                    DateAffectation = acteur.DateAffectation
                });
            }

            return dtos;
        }

        public async Task<ProcessusActeurDto> AddActeurAsync(Guid processusId, CreateProcessusActeurDto acteurDto)
        {
            if (acteurDto.UtilisateurId <= 0)
                throw new ArgumentException("L'identifiant de l'utilisateur est requis");

            if (string.IsNullOrWhiteSpace(acteurDto.TypeActeur))
                throw new ArgumentException("Le type d'acteur est requis");

            var processus = await _repository.GetByIdAsync(processusId);
            if (processus == null)
                throw new KeyNotFoundException("Processus non trouvé");

            var utilisateur = await _userRepository.GetByIdAsync(acteurDto.UtilisateurId);
            if (utilisateur == null)
                throw new KeyNotFoundException("Utilisateur non trouvé");

            var acteur = new ProcessusActeur
            {
                OrganisationId = processus.OrganisationId,
                ProcessusId = processusId,
                UtilisateurId = acteurDto.UtilisateurId,
                TypeActeur = Enum.Parse<TypeActeur>(acteurDto.TypeActeur, true)
            };

            var acteurId = await _repository.AddActeurAsync(acteur);

            return new ProcessusActeurDto
            {
                Id = acteurId,
                UtilisateurId = acteur.UtilisateurId,
                NomComplet = utilisateur.Username,
                Email = utilisateur.Email,
                TypeActeur = acteur.TypeActeur.ToString(),
                DateAffectation = acteur.DateAffectation
            };
        }

        public async Task<bool> RemoveActeurAsync(Guid acteurId)
        {
            return await _repository.RemoveActeurAsync(acteurId);
        }

        public async Task<int> GetProceduresCountAsync(Guid processusId)
        {
            return await _repository.GetProceduresCountAsync(processusId);
        }

        public async Task<int> GetDocumentsCountAsync(Guid processusId)
        {
            return await _repository.GetDocumentsCountAsync(processusId);
        }

        private async Task<ProcessusDto> MapToDto(Processus processus)
        {
            var proceduresCount = await _repository.GetProceduresCountAsync(processus.Id);
            var documentsCount = await _repository.GetDocumentsCountAsync(processus.Id);
            var acteurs = await _repository.GetActeursAsync(processus.Id);
            var pilote = await _userRepository.GetByIdAsync(processus.PiloteId);

            return new ProcessusDto
            {
                Id = processus.Id,
                Code = processus.Code,
                Nom = processus.Nom,
                Description = processus.Description,
                Type = processus.Type.ToString(),
                Finalites = string.IsNullOrEmpty(processus.Finalites)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(processus.Finalites) ?? new List<string>(),
                Perimetres = string.IsNullOrEmpty(processus.Perimetres)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(processus.Perimetres) ?? new List<string>(),
                Fournisseurs = string.IsNullOrEmpty(processus.Fournisseurs)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(processus.Fournisseurs) ?? new List<string>(),
                Clients = string.IsNullOrEmpty(processus.Clients)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(processus.Clients) ?? new List<string>(),
                DonneesEntree = string.IsNullOrEmpty(processus.DonneesEntree)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(processus.DonneesEntree) ?? new List<string>(),
                DonneesSortie = string.IsNullOrEmpty(processus.DonneesSortie)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(processus.DonneesSortie) ?? new List<string>(),
                Objectifs = string.IsNullOrEmpty(processus.Objectifs)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(processus.Objectifs) ?? new List<string>(),
                Pilote = pilote != null ? new ProcessusPiloteDto
                {
                    Id = pilote.Id,
                    NomComplet = pilote.Username,
                    Email = pilote.Email,
                    Fonction = pilote.Role
                } : null,
                Statut = processus.Statut.ToString(),
                ProceduresCount = proceduresCount,
                DocumentsCount = documentsCount,
                ActeursCount = acteurs?.Count() ?? 0,
                DateCreation = processus.DateCreation,
                DateModification = processus.DateModification
            };
        }

        public async Task<bool> CodeExistsAsync(string code, string organisationId, Guid? excludeId = null)
        {
            var orgId = Guid.Parse(organisationId);
            return await _repository.CodeExistsAsync(code, orgId, excludeId);
        }
    }
}
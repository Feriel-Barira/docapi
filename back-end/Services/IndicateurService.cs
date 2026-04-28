// Services/IndicateurService.cs
using DocApi.Domain.Entities;
using DocApi.DTOs;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocApi.Services
{
    public class IndicateurService : IIndicateurService
    {
        private readonly IIndicateurRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IProcessusRepository _processusRepository;

        public IndicateurService(
            IIndicateurRepository repository,
            IUserRepository userRepository,
            IProcessusRepository processusRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _processusRepository = processusRepository;
        }

        public async Task<IndicateurDto?> GetByIdAsync(Guid id)
        {
            var indicateur = await _repository.GetByIdAsync(id);
            if (indicateur == null) return null;
            return await MapToDto(indicateur);
        }

        public async Task<IEnumerable<IndicateurDto>> GetAllAsync(Guid organisationId)
        {
            var indicateurs = await _repository.GetAllAsync(organisationId);
            var dtos = new List<IndicateurDto>();
            foreach (var i in indicateurs)
            {
                dtos.Add(await MapToDto(i));
            }
            return dtos;
        }

        public async Task<IEnumerable<IndicateurDto>> GetByProcessusAsync(Guid processusId)
        {
            var indicateurs = await _repository.GetByProcessusAsync(processusId);
            var dtos = new List<IndicateurDto>();
            foreach (var i in indicateurs)
            {
                dtos.Add(await MapToDto(i));
            }
            return dtos;
        }

        public async Task<IEnumerable<IndicateurDto>> GetFilteredAsync(Guid organisationId, IndicateurFilterDto filter)
        {
            var actif = filter.Statut == "ACTIF" ? true : (filter.Statut == "INACTIF" ? false : (bool?)null);
            var indicateurs = await _repository.GetFilteredAsync(organisationId, filter.SearchTerm, actif);
            var dtos = new List<IndicateurDto>();
            foreach (var i in indicateurs)
            {
                if (filter.ProcessusId.HasValue && i.ProcessusId != filter.ProcessusId.Value)
                    continue;
                dtos.Add(await MapToDto(i));
            }

            if (filter.Page > 0 && filter.PageSize > 0)
            {
                dtos = dtos.Skip((filter.Page - 1) * filter.PageSize)
                           .Take(filter.PageSize)
                           .ToList();
            }

            return dtos;
        }

        public async Task<IndicateurDto> CreateAsync(Guid organisationId, Guid processusId, CreateIndicateurDto dto)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new ArgumentException("Le code de l'indicateur est requis");
            if (string.IsNullOrWhiteSpace(dto.Nom))
                throw new ArgumentException("Le nom de l'indicateur est requis");
            if (dto.ResponsableId <= 0)
                throw new ArgumentException("Le responsable est requis");

            // Vérifier le code
            var codeExists = await _repository.CodeExistsAsync(dto.Code, organisationId);
            if (codeExists)
                throw new InvalidOperationException($"Un indicateur avec le code {dto.Code} existe déjà");

            // Vérifier le processus
            var processus = await _processusRepository.GetByIdAsync(processusId);
            if (processus == null)
                throw new KeyNotFoundException("Processus non trouvé");

            // Vérifier le responsable
            var responsable = await _userRepository.GetByIdAsync(dto.ResponsableId);
            if (responsable == null)
                throw new KeyNotFoundException("Responsable non trouvé");

            var indicateur = new Indicateur
            {
                OrganisationId = organisationId,
                ProcessusId = processusId,
                Code = dto.Code,
                Nom = dto.Nom,
                Description = dto.Description,
                MethodeCalcul = dto.MethodeCalcul,
                Unite = dto.Unite,
                ValeurCible = dto.ValeurCible,
                SeuilAlerte = dto.SeuilAlerte,
                FrequenceMesure = Enum.Parse<FrequenceMesure>(dto.FrequenceMesure, true),
                ResponsableId = dto.ResponsableId,
                Actif = dto.Actif
            };

            var indicateurId = await _repository.CreateAsync(indicateur);
            var created = await _repository.GetByIdAsync(indicateurId);
            return await MapToDto(created);
        }

        public async Task<IndicateurDto> UpdateAsync(Guid id, UpdateIndicateurDto dto)
        {
            var indicateur = await _repository.GetByIdAsync(id);
            if (indicateur == null)
                throw new KeyNotFoundException("Indicateur non trouvé");

            if (!string.IsNullOrWhiteSpace(dto.Code) && dto.Code != indicateur.Code)
            {
                var codeExists = await _repository.CodeExistsAsync(dto.Code, indicateur.OrganisationId, id);
                if (codeExists)
                    throw new InvalidOperationException($"Un indicateur avec le code {dto.Code} existe déjà");
                indicateur.Code = dto.Code;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nom))
                indicateur.Nom = dto.Nom;
            if (dto.Description != null)
                indicateur.Description = dto.Description;
            if (dto.MethodeCalcul != null)
                indicateur.MethodeCalcul = dto.MethodeCalcul;
            if (dto.Unite != null)
                indicateur.Unite = dto.Unite;
            if (dto.ValeurCible.HasValue)
                indicateur.ValeurCible = dto.ValeurCible;
            if (dto.SeuilAlerte.HasValue)
                indicateur.SeuilAlerte = dto.SeuilAlerte;
            if (!string.IsNullOrWhiteSpace(dto.FrequenceMesure))
                indicateur.FrequenceMesure = Enum.Parse<FrequenceMesure>(dto.FrequenceMesure, true);
            if (dto.ResponsableId.HasValue)
                indicateur.ResponsableId = dto.ResponsableId.Value;
            if (dto.Actif.HasValue)
                indicateur.Actif = dto.Actif.Value;
            if (dto.ProcessusId.HasValue && dto.ProcessusId.Value != Guid.Empty)
            {
                var processus = await _processusRepository.GetByIdAsync(dto.ProcessusId.Value);
                if (processus == null)
                    throw new KeyNotFoundException("Processus non trouvé");
                indicateur.ProcessusId = dto.ProcessusId.Value;
            }

            await _repository.UpdateAsync(indicateur);
            var updated = await _repository.GetByIdAsync(id);
            return await MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new KeyNotFoundException("Indicateur non trouvé");
            return await _repository.DeleteAsync(id);
        }

        // ==================== VALEURS ====================

        public async Task<IEnumerable<IndicateurValeurDto>> GetValeursAsync(Guid indicateurId, int limit = 12)
        {
            var valeurs = await _repository.GetValeursAsync(indicateurId, limit);
            var dtos = new List<IndicateurValeurDto>();
            foreach (var v in valeurs)
            {
                dtos.Add(await MapValeurToDto(v));
            }
            return dtos.OrderBy(v => v.Periode);
        }

        public async Task<IndicateurValeurDto> AddValeurAsync(Guid indicateurId, CreateIndicateurValeurDto dto, int userId)
        {
            var indicateur = await _repository.GetByIdAsync(indicateurId);
            if (indicateur == null)
                throw new KeyNotFoundException("Indicateur non trouvé");

            var existing = await _repository.GetValeurByPeriodeAsync(indicateurId, dto.Periode);
            if (existing != null)
                throw new InvalidOperationException($"Une valeur existe déjà pour la période {dto.Periode}");

            var valeur = new IndicateurValeur
            {
                IndicateurId = indicateurId,
                Periode = dto.Periode,
                Valeur = dto.Valeur,
                Commentaire = dto.Commentaire,
                SaisiParId = userId,
                OrganisationId = indicateur.OrganisationId,
                // ← AJOUTER : utiliser la date du DTO si fournie
                DateMesure = !string.IsNullOrEmpty(dto.DateMesure)
            ? DateTime.Parse(dto.DateMesure)
            : DateTime.UtcNow
            };

            var valeurId = await _repository.AddValeurAsync(valeur);
            var created = await _repository.GetValeurByPeriodeAsync(indicateurId, dto.Periode);
            return await MapValeurToDto(created);
        }

        public async Task<IndicateurValeurDto> UpdateValeurAsync(Guid valeurId, CreateIndicateurValeurDto dto)
        {
            var valeur = await _repository.GetValeurByPeriodeAsync(valeurId, dto.Periode);
            if (valeur == null)
                throw new KeyNotFoundException("Valeur non trouvée");

            valeur.Valeur = dto.Valeur;
            valeur.Commentaire = dto.Commentaire;

            await _repository.UpdateValeurAsync(valeur);
            return await MapValeurToDto(valeur);
        }

        public async Task<bool> DeleteValeurAsync(Guid valeurId)
        {
            return await _repository.DeleteValeurAsync(valeurId);
        }

        // ==================== MAPPING ====================
        private async Task<IndicateurDto> MapToDto(Indicateur indicateur)
        {
            // Charger le processus
            var processus = await _processusRepository.GetByIdAsync(indicateur.ProcessusId);

            // Charger le responsable
            var responsable = await _userRepository.GetByIdAsync(indicateur.ResponsableId);

            // Dernière valeur
            var derniereValeur = await _repository.GetDerniereValeurAsync(indicateur.Id);
            var tendance = await _repository.GetTendanceAsync(indicateur.Id);
            var valeurs = await _repository.GetValeursAsync(indicateur.Id, 6);

            return new IndicateurDto
            {
                Id = indicateur.Id,
                Code = indicateur.Code,
                Nom = indicateur.Nom,
                Description = indicateur.Description,
                MethodeCalcul = indicateur.MethodeCalcul,
                Unite = indicateur.Unite,
                ValeurCible = indicateur.ValeurCible,
                Actif = indicateur.Actif,  // ← AJOUTER CETTE LIGNE
                Statut = indicateur.Actif ? "ACTIF" : "INACTIF",
                SeuilAlerte = indicateur.SeuilAlerte,
                FrequenceMesure = indicateur.FrequenceMesure.ToString(),
                Responsable = responsable != null ? new IndicateurResponsableDto
                {
                    Id = responsable.Id,
                    NomComplet = responsable.Username,
                    Email = responsable.Email
                } : null,
                // ✅ AJOUTER CECI - Le processus
                Processus = processus != null ? new IndicateurProcessusDto
                {
                    Id = processus.Id,
                    Code = processus.Code,
                    Nom = processus.Nom
                } : null,
                DerniereValeur = derniereValeur?.Valeur,
                DernierePeriode = derniereValeur?.Periode,
                Tendance = tendance,
                Valeurs = valeurs.Select(v => new IndicateurValeurDto
                {
                    Id = v.Id,
                    Periode = v.Periode,
                    Valeur = v.Valeur,
                    Commentaire = v.Commentaire,
                    DateMesure = v.DateMesure
                }).ToList(),
                DateCreation = indicateur.DateCreation
            };
        }

        private async Task<IndicateurValeurDto> MapValeurToDto(IndicateurValeur? valeur)
        {
            if (valeur == null) return null;

            var saisiPar = await _userRepository.GetByIdAsync(valeur.SaisiParId);

            return new IndicateurValeurDto
            {
                Id = valeur.Id,
                Periode = valeur.Periode,
                Valeur = valeur.Valeur,
                Commentaire = valeur.Commentaire,
                DateMesure = valeur.DateMesure,
                SaisiPar = saisiPar != null ? new IndicateurResponsableDto
                {
                    Id = saisiPar.Id,
                    NomComplet = saisiPar.Username,
                    Email = saisiPar.Email
                } : null
            };
        }
    }
}
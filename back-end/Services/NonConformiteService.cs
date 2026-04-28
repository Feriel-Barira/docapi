// Services/NonConformiteService.cs
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
    public class NonConformiteService : INonConformiteService
    {
        private readonly INonConformiteRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IProcessusRepository _processusRepository;
        private readonly IActionCorrectiveRepository _actionCorrectiveRepository;

        public NonConformiteService(
            INonConformiteRepository repository,
            IUserRepository userRepository,
            IProcessusRepository processusRepository,
            IActionCorrectiveRepository actionCorrectiveRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _processusRepository = processusRepository;
            _actionCorrectiveRepository = actionCorrectiveRepository;
        }

        public async Task<NonConformiteDto?> GetByIdAsync(Guid id)
        {
            var nc = await _repository.GetByIdAsync(id);
            if (nc == null) return null;
            return await MapToDto(nc);
        }

        public async Task<IEnumerable<NonConformiteDto>> GetAllAsync(Guid organisationId, string userRole, int userId)
        {
            var ncs = await _repository.GetAllAsync(organisationId, userRole, userId);
            var dtos = new List<NonConformiteDto>();
            foreach (var nc in ncs)
            {
                dtos.Add(await MapToDto(nc));
            }
            return dtos;
        }

        public async Task<IEnumerable<NonConformiteDto>> GetByProcessusAsync(Guid processusId)
        {
            var ncs = await _repository.GetByProcessusAsync(processusId);
            var dtos = new List<NonConformiteDto>();
            foreach (var nc in ncs)
            {
                dtos.Add(await MapToDto(nc));
            }
            return dtos;
        }

        public async Task<IEnumerable<NonConformiteDto>> GetFilteredAsync(Guid organisationId, NonConformiteFilterDto filter)
        {
            var filterParams = new NonConformiteFilterParams
            {
                ProcessusId = filter.ProcessusId,
                Source = filter.Source,
                Gravite = filter.Gravite,
                Statut = filter.Statut,
                DateDebut = filter.DateDebut,
                DateFin = filter.DateFin
            };

            var ncs = await _repository.GetFilteredAsync(organisationId, filterParams);
            var dtos = new List<NonConformiteDto>();
            foreach (var nc in ncs)
            {
                dtos.Add(await MapToDto(nc));
            }

            if (filter.Page > 0 && filter.PageSize > 0)
            {
                dtos = dtos.Skip((filter.Page - 1) * filter.PageSize)
                           .Take(filter.PageSize)
                           .ToList();
            }

            return dtos;
        }

        public async Task<NonConformiteDto> CreateAsync(Guid organisationId, CreateNonConformiteDto dto, int userId)
        {
            // Validation
            if (dto.ProcessusId == Guid.Empty)
                throw new ArgumentException("Le processus est requis");
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("La description est requise");

            // Vérifier le processus
            var processus = await _processusRepository.GetByIdAsync(dto.ProcessusId);
            if (processus == null)
                throw new KeyNotFoundException("Processus non trouvé");

            // Générer la référence
            var reference = await _repository.GenerateReferenceAsync(organisationId, dto.ProcessusId);

            // Créer la non-conformité
            var nc = new NonConformite
            {
                OrganisationId = organisationId,
                ProcessusId = dto.ProcessusId,
                Reference = reference,
                Description = dto.Description,
                Source = Enum.Parse<NonConformiteSource>(dto.Source, true),
                Gravite = Enum.Parse<NonConformiteGravite>(dto.Gravite, true),
                DetecteParId = dto.DetecteParId,
                DateDetection = dto.DateDetection,
                ResponsableTraitementId = dto.ResponsableTraitementId,
                Statut = NonConformiteStatut.OUVERTE,
                Type = dto.Type ?? "PRODUIT_SERVICE",    // ← AJOUTER
                Nature = dto.Nature ?? "REELLE"
            };

            var ncId = await _repository.CreateAsync(nc);
            var created = await _repository.GetByIdAsync(ncId);
            return await MapToDto(created);
        }
        public async Task<NonConformiteDto> UpdateAsync(Guid id, UpdateNonConformiteDto dto)
        {
            var nc = await _repository.GetByIdAsync(id);
            if (nc == null)
                throw new KeyNotFoundException($"Non-conformité avec l'ID {id} non trouvée");

            // Mettre à jour tous les champs
            if (!string.IsNullOrWhiteSpace(dto.Description))
                nc.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Type))
                nc.Type = dto.Type;

            if (!string.IsNullOrWhiteSpace(dto.Nature))
                nc.Nature = dto.Nature;

            if (!string.IsNullOrWhiteSpace(dto.Source))
                nc.Source = Enum.Parse<NonConformiteSource>(dto.Source, true);

            if (!string.IsNullOrWhiteSpace(dto.Gravite))
                nc.Gravite = Enum.Parse<NonConformiteGravite>(dto.Gravite, true);

            if (dto.ProcessusId.HasValue && dto.ProcessusId.Value != Guid.Empty)
                nc.ProcessusId = dto.ProcessusId.Value;

            if (dto.ResponsableTraitementId.HasValue)
                nc.ResponsableTraitementId = dto.ResponsableTraitementId;

            if (dto.DetecteParId.HasValue)
                nc.DetecteParId = dto.DetecteParId.Value;

            if (dto.DateDetection.HasValue)
                nc.DateDetection = dto.DateDetection.Value;

            var updated = await _repository.UpdateAsync(nc);
            if (!updated)
                throw new Exception("Erreur lors de la mise à jour");

            var updatedNc = await _repository.GetByIdAsync(id);
            return await MapToDto(updatedNc);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new KeyNotFoundException($"Non-conformité avec l'ID {id} non trouvée");
            return await _repository.DeleteAsync(id);
        }

        public async Task<NonConformiteDto> UpdateStatutAsync(Guid id, string nouveauStatut, int userId, string? commentaire)
        {
            var nc = await _repository.GetByIdAsync(id);
            if (nc == null)
                throw new KeyNotFoundException($"Non-conformité avec l'ID {id} non trouvée");

            var statut = Enum.Parse<NonConformiteStatut>(nouveauStatut, true);
            var updated = await _repository.UpdateStatutAsync(id, statut, userId, commentaire);

            if (!updated)
                throw new Exception("Erreur lors de la mise à jour du statut");

            var updatedNc = await _repository.GetByIdAsync(id);
            return await MapToDto(updatedNc);
        }

        // ==================== ANALYSES ====================

        public async Task<AnalyseCauseDto> AddAnalyseAsync(Guid nonConformiteId, CreateAnalyseCauseDto dto, int userId)
        {
            var nc = await _repository.GetByIdAsync(nonConformiteId);
            if (nc == null)
                throw new KeyNotFoundException("Non-conformité non trouvée");

            // ✅ Vérifier si une analyse existe déjà pour cette NC
            var existing = await _repository.GetAnalyseByNonConformiteIdAsync(nonConformiteId);
            if (existing != null)
            {
                // Mettre à jour au lieu de créer
                existing.MethodeAnalyse = Enum.Parse<AnalyseMethode>(dto.MethodeAnalyse, true);
                existing.Description = dto.Description;
                await _repository.UpdateAnalyseAsync(existing);
                return await MapAnalyseToDto(existing);
            }

            // Créer seulement si aucune analyse n'existe
            var analyse = new AnalyseCauseNonConformite
            {
                NonConformiteId = nonConformiteId,
                MethodeAnalyse = Enum.Parse<AnalyseMethode>(dto.MethodeAnalyse, true),
                Description = dto.Description,
                AnalyseParId = userId
            };

            await _repository.AddAnalyseAsync(analyse);
            var created = await _repository.GetAnalyseByNonConformiteIdAsync(nonConformiteId);
            return await MapAnalyseToDto(created);
        }

        public async Task<AnalyseCauseDto> UpdateAnalyseAsync(Guid analyseId, CreateAnalyseCauseDto dto)
        {
            // ✅ Chercher par l'Id de l'analyse, pas par NonConformiteId
            var analyse = await _repository.GetAnalyseByIdAsync(analyseId);
            if (analyse == null)
                throw new KeyNotFoundException("Analyse non trouvée");

            analyse.MethodeAnalyse = Enum.Parse<AnalyseMethode>(dto.MethodeAnalyse, true);
            analyse.Description = dto.Description;

            await _repository.UpdateAnalyseAsync(analyse);
            return await MapAnalyseToDto(analyse);
        }
        // ==================== HISTORIQUE ====================

        public async Task<IEnumerable<HistoriqueNonConformiteDto>> GetHistoriqueAsync(Guid nonConformiteId)
        {
            var historique = await _repository.GetHistoriqueAsync(nonConformiteId);
            var dtos = new List<HistoriqueNonConformiteDto>();
            foreach (var h in historique)
            {
                dtos.Add(await MapHistoriqueToDto(h));
            }
            return dtos;
        }

        // ==================== MAPPING ====================
        private async Task<NonConformiteDto> MapToDto(NonConformite nc)
        {
            var processus = await _processusRepository.GetByIdAsync(nc.ProcessusId);
            var detectePar = await _userRepository.GetByIdAsync(nc.DetecteParId);
            var responsable = nc.ResponsableTraitementId.HasValue
                ? await _userRepository.GetByIdAsync(nc.ResponsableTraitementId.Value)
                : null;

            // ✅ CORRECTION : Décommentez cette ligne pour charger l'analyse
            var analyse = await _repository.GetAnalyseByNonConformiteIdAsync(nc.Id);

            var historique = await _repository.GetHistoriqueAsync(nc.Id);

            // Charger les actions correctives
            var actions = await _actionCorrectiveRepository.GetByNonConformiteAsync(nc.Id);
            var actionsDto = new List<ActionCorrectiveDto>();
            foreach (var action in actions)
            {
                var responsableAction = await _userRepository.GetByIdAsync(action.ResponsableId);
                actionsDto.Add(new ActionCorrectiveDto
                {
                    Id = action.Id,
                    Type = action.Type.ToString(),
                    Description = action.Description,
                    Responsable = responsableAction != null ? new ActionCorrectiveResponsableDto
                    {
                        Id = responsableAction.Id,
                        NomComplet = $"{responsableAction.Prenom} {responsableAction.Nom}".Trim(),
                        Email = responsableAction.Email
                    } : null,
                    DateEcheance = action.DateEcheance,
                    Statut = action.Statut.ToString(),
                    DateCreation = action.DateCreation,
                    PreuveEnregistrementId = action.PreuveEnregistrementId
                });
            }

            return new NonConformiteDto
            {
                Id = nc.Id,
                Reference = nc.Reference,
                Description = nc.Description,
                DateDetection = nc.DateDetection,
                Source = nc.Source.ToString(),
                Gravite = nc.Gravite.ToString(),
                Statut = nc.Statut.ToString(),
                // ✅ AJOUTER Type et Nature
                Type = nc.Type,
                Nature = nc.Nature,
                Processus = processus != null ? new NonConformiteProcessusDto
                {
                    Id = processus.Id,
                    Code = processus.Code,
                    Nom = processus.Nom
                } : null,
                DetectePar = detectePar != null ? new NonConformiteUtilisateurDto
                {
                    Id = detectePar.Id,
                    NomComplet = $"{detectePar.Prenom} {detectePar.Nom}".Trim(),
                    Email = detectePar.Email,
                    Fonction = detectePar.Fonction ?? detectePar.Role
                } : null,
                ResponsableTraitement = responsable != null ? new NonConformiteUtilisateurDto
                {
                    Id = responsable.Id,
                    NomComplet = $"{responsable.Prenom} {responsable.Nom}".Trim(),
                    Email = responsable.Email,
                    Fonction = responsable.Fonction ?? responsable.Role
                } : null,
                AnalyseCause = analyse != null ? await MapAnalyseToDto(analyse) : null,
                ActionsCorrectives = actionsDto,
                Historique = historique.Select(h => new HistoriqueNonConformiteDto
                {
                    Id = h.Id,
                    AncienStatut = h.AncienStatut.ToString(),
                    NouveauStatut = h.NouveauStatut.ToString(),
                    DateChangement = h.DateChangement,
                    Commentaire = h.Commentaire
                }).ToList(),
                DateCloture = nc.DateCloture,
                DateCreation = nc.DateCreation,
                DateModification = nc.DateModification
            };
        }


        private async Task<AnalyseCauseDto> MapAnalyseToDto(AnalyseCauseNonConformite? analyse)
        {
            if (analyse == null) return null;

            var analysePar = await _userRepository.GetByIdAsync(analyse.AnalyseParId);

            return new AnalyseCauseDto
            {
                Id = analyse.Id,
                MethodeAnalyse = analyse.MethodeAnalyse.ToString().Replace("_", ""),
                Description = analyse.Description,
                DateAnalyse = analyse.DateAnalyse,
                AnalysePar = analysePar != null ? new NonConformiteUtilisateurDto
                {
                    Id = analysePar.Id,
                    NomComplet = $"{analysePar.Prenom} {analysePar.Nom}".Trim(),
                    Email = analysePar.Email,
                    Fonction = analysePar.Fonction ?? analysePar.Role
                } : null
            };
        }
        private async Task<HistoriqueNonConformiteDto> MapHistoriqueToDto(HistoriqueNonConformite h)
        {
            var changePar = await _userRepository.GetByIdAsync(h.ChangeParId);

            return new HistoriqueNonConformiteDto
            {
                Id = h.Id,
                AncienStatut = h.AncienStatut.ToString(),
                NouveauStatut = h.NouveauStatut.ToString(),
                DateChangement = h.DateChangement,
                ChangePar = changePar != null ? new NonConformiteUtilisateurDto
                {
                    Id = changePar.Id,
                    NomComplet = changePar.Username,
                    Email = changePar.Email,
                    Fonction = changePar.Role
                } : null,
                Commentaire = h.Commentaire
            };
        }
    }
}
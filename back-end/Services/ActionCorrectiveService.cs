// Services/ActionCorrectiveService.cs
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
    public class ActionCorrectiveService : IActionCorrectiveService
    {
        private readonly IActionCorrectiveRepository _repository;
        private readonly INonConformiteRepository _nonConformiteRepository;
        private readonly IUserRepository _userRepository;

        public ActionCorrectiveService(
            IActionCorrectiveRepository repository,
            INonConformiteRepository nonConformiteRepository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _nonConformiteRepository = nonConformiteRepository;
            _userRepository = userRepository;
        }

        public async Task<ActionCorrectiveDto?> GetByIdAsync(Guid id)
        {
            var action = await _repository.GetByIdAsync(id);
            if (action == null) return null;
            return await MapToDto(action);
        }

        public async Task<IEnumerable<ActionCorrectiveDto>> GetByNonConformiteAsync(Guid nonConformiteId)
        {
            var actions = await _repository.GetByNonConformiteAsync(nonConformiteId);
            var dtos = new List<ActionCorrectiveDto>();
            foreach (var action in actions)
            {
                dtos.Add(await MapToDto(action));
            }
            return dtos;
        }

        public async Task<ActionCorrectiveDto> CreateAsync(Guid nonConformiteId, CreateActionCorrectiveDto dto)
        {
            Console.WriteLine($"=== CREATE ACTION CORRECTIVE ===");
            Console.WriteLine($"DateEcheance reçue: {dto.DateEcheance}");
            Console.WriteLine($"DateEcheance.Kind: {dto.DateEcheance.Kind}");
            Console.WriteLine($"DateTime.Now: {DateTime.Now}");
            // Validation
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("La description est requise");
            if (dto.ResponsableId <= 0)
                throw new ArgumentException("Le responsable est requis");
            // Comparer uniquement la date, sans l'heure
            if (dto.DateEcheance.ToUniversalTime().Date <= DateTime.UtcNow.Date)
                throw new ArgumentException("La date d'échéance doit être dans le futur");

            // Vérifier la NC
            var nc = await _nonConformiteRepository.GetByIdAsync(nonConformiteId);
            if (nc == null)
                throw new KeyNotFoundException("Non-conformité non trouvée");

            // Vérifier le responsable
            var responsable = await _userRepository.GetByIdAsync(dto.ResponsableId);
            if (responsable == null)
                throw new KeyNotFoundException("Responsable non trouvé");

            var action = new ActionCorrective
            {
                NonConformiteId = nonConformiteId,
                Type = Enum.Parse<ActionCorrectiveType>(dto.Type, true),
                Description = dto.Description,
                ResponsableId = dto.ResponsableId,
                DateEcheance = dto.DateEcheance
            };

            var actionId = await _repository.CreateAsync(action);

            // Mettre à jour le statut de la NC
            // await _nonConformiteRepository.UpdateStatutAsync(nonConformiteId, NonConformiteStatut.ACTION_EN_COURS, 1, "Action corrective planifiée");

            var created = await _repository.GetByIdAsync(actionId);
            return await MapToDto(created);
        }

        public async Task<ActionCorrectiveDto> UpdateAsync(Guid id, UpdateActionCorrectiveDto dto)
        {
            var action = await _repository.GetByIdAsync(id);
            if (action == null)
                throw new KeyNotFoundException("Action corrective non trouvée");

            if (!string.IsNullOrWhiteSpace(dto.Description))
                action.Description = dto.Description;
            if (dto.ResponsableId.HasValue)
                action.ResponsableId = dto.ResponsableId.Value;
            if (dto.DateEcheance.HasValue)
                action.DateEcheance = dto.DateEcheance.Value;
            if (!string.IsNullOrWhiteSpace(dto.Statut))
            {
                action.Statut = Enum.Parse<ActionCorrectiveStatut>(dto.Statut, true);

                // ✅ Auto-remplir DateRealisation quand statut = REALISEE
                if (action.Statut == ActionCorrectiveStatut.REALISEE && !action.DateRealisation.HasValue)
                    action.DateRealisation = DateTime.UtcNow;

                // ✅ Auto-remplir DateVerification quand statut = VERIFIEE  
                //if (action.Statut == ActionCorrectiveStatut.VERIFIEE && !action.DateVerification.HasValue)
                //action.DateVerification = DateTime.UtcNow;
            }
            //if (dto.CommentaireRealisation != null)
            //action.CommentaireRealisation = dto.CommentaireRealisation;
            if (dto.DateRealisation.HasValue)
                action.DateRealisation = dto.DateRealisation.Value;

            await _repository.UpdateAsync(action);

            var updated = await _repository.GetByIdAsync(id);
            return await MapToDto(updated);
        }

        public async Task<ActionCorrectiveDto> RealiserAsync(Guid id, string commentaire, int userId)
        {
            var action = await _repository.GetByIdAsync(id);
            if (action == null)
                throw new KeyNotFoundException("Action corrective non trouvée");

            action.Statut = ActionCorrectiveStatut.REALISEE;
            action.DateRealisation = DateTime.UtcNow;
            //action.CommentaireRealisation = commentaire;

            await _repository.UpdateAsync(action);

            var updated = await _repository.GetByIdAsync(id);
            return await MapToDto(updated);
        }

        public async Task<ActionCorrectiveDto> VerifierAsync(Guid id, bool efficace, string? commentaire, int userId)
        {
            var action = await _repository.GetByIdAsync(id);
            if (action == null)
                throw new KeyNotFoundException("Action corrective non trouvée");

            action.Statut = ActionCorrectiveStatut.VERIFIEE;
            // action.VerifieParId = userId;
            // action.DateVerification = DateTime.UtcNow;
            // action.CommentaireVerification = commentaire;

            await _repository.UpdateAsync(action);

            // Vérifier si toutes les actions de la NC sont vérifiées
            var actions = await _repository.GetByNonConformiteAsync(action.NonConformiteId);
            var toutesVerifiees = actions.All(a => a.Statut == ActionCorrectiveStatut.VERIFIEE);

            if (toutesVerifiees && efficace)
            {
                await _nonConformiteRepository.UpdateStatutAsync(action.NonConformiteId, NonConformiteStatut.CLOTUREE, userId, "Toutes les actions correctives ont été vérifiées avec succès");
            }

            var updated = await _repository.GetByIdAsync(id);
            return await MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new KeyNotFoundException("Action corrective non trouvée");
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ActionCorrectiveDto>> GetEcheanceProcheAsync(Guid organisationId, int joursAlerte = 7)
        {
            var actions = await _repository.GetEcheanceProcheAsync(organisationId, joursAlerte);
            var dtos = new List<ActionCorrectiveDto>();
            foreach (var action in actions)
            {
                dtos.Add(await MapToDto(action));
            }
            return dtos;
        }

        private async Task<ActionCorrectiveDto> MapToDto(ActionCorrective action)
        {
            var responsable = await _userRepository.GetByIdAsync(action.ResponsableId);
            /*var verifiePar = action.VerifieParId.HasValue
                ? await _userRepository.GetByIdAsync(action.VerifieParId.Value)
                : null;*/

            return new ActionCorrectiveDto
            {
                Id = action.Id,
                Type = action.Type.ToString(),
                Description = action.Description,
                Responsable = responsable != null ? new ActionCorrectiveResponsableDto
                {
                    Id = responsable.Id,
                    NomComplet = $"{responsable.Prenom} {responsable.Nom}".Trim(),  // ← CORRIGÉ
                    Email = responsable.Email
                } : null,
                DateEcheance = action.DateEcheance,
                Statut = action.Statut.ToString(),
                DateRealisation = action.DateRealisation,
                //  CommentaireRealisation = action.CommentaireRealisation,
                //VerifiePar = verifiePar != null ? new ActionCorrectiveResponsableDto
                /*{
                    Id = verifiePar.Id,
                    NomComplet = $"{verifiePar.Prenom} {verifiePar.Nom}".Trim(),    // ← CORRIGÉ
                    Email = verifiePar.Email
                } : null,*/
                //DateVerification = action.DateVerification,
                //CommentaireVerification = action.CommentaireVerification,
                DateCreation = action.DateCreation,
                PreuveEnregistrementId = action.PreuveEnregistrementId
            };
        }
        public async Task AttacherPreuveAsync(Guid id, Guid enregistrementId)
        {
            var action = await _repository.GetByIdAsync(id);
            if (action == null) throw new KeyNotFoundException("Action corrective non trouvée");
            action.PreuveEnregistrementId = enregistrementId;
            await _repository.UpdateAsync(action);
        }
        public async Task DetacherPreuveAsync(Guid id)
        {
            var action = await _repository.GetByIdAsync(id);
            if (action == null) throw new KeyNotFoundException("Action corrective non trouvée");
            action.PreuveEnregistrementId = null;
            await _repository.UpdateAsync(action);
        }
    }
}
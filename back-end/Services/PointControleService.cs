using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocApi.Domain;
using DocApi.DTOs;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;


namespace DocApi.Services
{
    public class PointControleService : IPointControleService
    {
        private readonly IPointControleRepository _repository;
        private readonly IUserRepository _userRepository;           // ← Doit exister
        private readonly IProcessusRepository _processusRepository;

        public PointControleService(IPointControleRepository repository,
        IUserRepository userRepository,
    IProcessusRepository processusRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _processusRepository = processusRepository;
        }

        public async Task<IEnumerable<PointControleResponseDto>> GetByOrganisationIdAsync(Guid organisationId)
        {
            var pointsControle = await _repository.GetByOrganisationIdAsync(organisationId);
            var result = new List<PointControleResponseDto>();

            foreach (var pc in pointsControle)
            {
                // 1️⃣ Récupérer les évaluations avec les noms des évaluateurs
                var evaluations = await _repository.GetEvaluationsByPointControleIdAsync(pc.Id);
                var evaluationDtos = new List<EvaluationResponseDto>();

                foreach (var eval in evaluations)
                {
                    string? evalueParNom = null;
                    if (eval.EvalueParId.HasValue)
                    {
                        var user = await _userRepository.GetByIdAsync(eval.EvalueParId.Value);
                        evalueParNom = user != null ? $"{user.Prenom} {user.Nom}" : null;
                    }

                    evaluationDtos.Add(new EvaluationResponseDto
                    {
                        Id = eval.Id,
                        PointControleId = eval.PointControleId,
                        DateEvaluation = eval.DateEvaluation,
                        Conforme = eval.Conforme,
                        Commentaire = eval.Commentaire,
                        EvalueParId = eval.EvalueParId,
                        EvalueParNom = evalueParNom,
                        DateCreation = eval.DateCreation
                    });
                }

                // 2️⃣ Récupérer le nom du responsable
                string? responsableNom = null;
                if (pc.ResponsableId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(pc.ResponsableId.Value);
                    responsableNom = user != null ? $"{user.Prenom} {user.Nom}" : null;
                }

                // 3️⃣ Récupérer le code et le nom du processus
                string? processusCode = null;
                string? processusNom = null;
                if (pc.ProcessusId.HasValue)
                {
                    var processus = await _processusRepository.GetByIdAsync(pc.ProcessusId.Value);
                    processusCode = processus?.Code;
                    processusNom = processus?.Nom;
                }

                // 4️⃣ Construire le DTO final
                result.Add(new PointControleResponseDto
                {
                    Id = pc.Id,
                    OrganisationId = pc.OrganisationId,
                    ProcessusId = pc.ProcessusId,
                    ProcessusCode = processusCode,
                    ProcessusNom = processusNom,
                    Nom = pc.Nom,
                    Description = pc.Description,
                    Type = pc.Type,
                    Frequence = pc.Frequence,
                    ResponsableId = pc.ResponsableId,
                    ResponsableNom = responsableNom,
                    Actif = pc.Actif,
                    DateCreation = pc.DateCreation,
                    DateModification = pc.DateModification,
                    Evaluations = evaluationDtos,
                    DerniereEvaluation = evaluationDtos.FirstOrDefault()
                });
            }

            return result;
        }

        public async Task<PointControleResponseDto?> GetByIdAsync(Guid id)
        {
            var pc = await _repository.GetByIdAsync(id);
            if (pc == null) return null;

            // 1️⃣ Récupérer les évaluations avec les noms des évaluateurs
            var evaluations = await _repository.GetEvaluationsByPointControleIdAsync(pc.Id);
            var evaluationDtos = new List<EvaluationResponseDto>();

            foreach (var eval in evaluations)
            {
                string? evalueParNom = null;
                if (eval.EvalueParId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(eval.EvalueParId.Value);
                    evalueParNom = user != null ? $"{user.Prenom} {user.Nom}" : null;
                }

                evaluationDtos.Add(new EvaluationResponseDto
                {
                    Id = eval.Id,
                    PointControleId = eval.PointControleId,
                    DateEvaluation = eval.DateEvaluation,
                    Conforme = eval.Conforme,
                    Commentaire = eval.Commentaire,
                    EvalueParId = eval.EvalueParId,
                    EvalueParNom = evalueParNom,
                    DateCreation = eval.DateCreation
                });
            }

            // 2️⃣ Récupérer le nom du responsable
            string? responsableNom = null;
            if (pc.ResponsableId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(pc.ResponsableId.Value);
                responsableNom = user != null ? $"{user.Prenom} {user.Nom}" : null;
            }

            // 3️⃣ Récupérer le code et le nom du processus
            string? processusCode = null;
            string? processusNom = null;
            if (pc.ProcessusId.HasValue)
            {
                var processus = await _processusRepository.GetByIdAsync(pc.ProcessusId.Value);
                processusCode = processus?.Code;
                processusNom = processus?.Nom;
            }

            // 4️⃣ Construire le DTO final
            return new PointControleResponseDto
            {
                Id = pc.Id,
                OrganisationId = pc.OrganisationId,
                ProcessusId = pc.ProcessusId,
                ProcessusCode = processusCode,
                ProcessusNom = processusNom,
                Nom = pc.Nom,
                Description = pc.Description,
                Type = pc.Type,
                Frequence = pc.Frequence,
                ResponsableId = pc.ResponsableId,
                ResponsableNom = responsableNom,
                Actif = pc.Actif,
                DateCreation = pc.DateCreation,
                DateModification = pc.DateModification,
                Evaluations = evaluationDtos,
                DerniereEvaluation = evaluationDtos.FirstOrDefault()
            };
        }

        public async Task<PointControleResponseDto> CreateAsync(Guid organisationId, PointControleCreateDto dto)
        {
            var pointControle = new PointControle
            {
                Id = Guid.NewGuid(),
                OrganisationId = organisationId,
                ProcessusId = dto.ProcessusId,
                Nom = dto.Nom,
                Description = dto.Description,
                Type = dto.Type,
                Frequence = dto.Frequence,
                ResponsableId = dto.ResponsableId,
                Actif = dto.Actif,
                DateCreation = DateTime.UtcNow
            };

            await _repository.CreateAsync(pointControle);
            return await GetByIdAsync(pointControle.Id) ?? throw new Exception("Failed to retrieve created point controle");
        }

        public async Task<PointControleResponseDto?> UpdateAsync(Guid id, PointControleCreateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Nom = dto.Nom;
            existing.Description = dto.Description;
            existing.ProcessusId = dto.ProcessusId;
            existing.Type = dto.Type;
            existing.Frequence = dto.Frequence;
            existing.ResponsableId = dto.ResponsableId;
            existing.Actif = dto.Actif;
            existing.DateModification = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<EvaluationResponseDto>> GetEvaluationsAsync(Guid pointControleId)
        {
            var evaluations = await _repository.GetEvaluationsByPointControleIdAsync(pointControleId);
            return evaluations.Select(eval => new EvaluationResponseDto
            {
                Id = eval.Id,
                PointControleId = eval.PointControleId,
                DateEvaluation = eval.DateEvaluation,
                Conforme = eval.Conforme,
                Commentaire = eval.Commentaire,
                EvalueParId = eval.EvalueParId,
                DateCreation = eval.DateCreation
            }).OrderByDescending(e => e.DateEvaluation);
        }

        public async Task<EvaluationResponseDto> AddEvaluationAsync(Guid pointControleId, EvaluationCreateDto dto)
        {
            var evaluation = new EvaluationPointControle
            {
                Id = Guid.NewGuid(),
                PointControleId = pointControleId,
                DateEvaluation = dto.DateEvaluation,
                Conforme = dto.Conforme,
                Commentaire = dto.Commentaire,
                EvalueParId = dto.EvalueParId,
                DateCreation = DateTime.UtcNow
            };

            await _repository.AddEvaluationAsync(evaluation);

            // Récupérer le nom de l’évaluateur
            string? evalueParNom = null;
            if (dto.EvalueParId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(dto.EvalueParId.Value);
                evalueParNom = user != null ? $"{user.Prenom} {user.Nom}" : null;
            }

            return new EvaluationResponseDto
            {
                Id = evaluation.Id,
                PointControleId = evaluation.PointControleId,
                DateEvaluation = evaluation.DateEvaluation,
                Conforme = evaluation.Conforme,
                Commentaire = evaluation.Commentaire,
                EvalueParId = evaluation.EvalueParId,
                EvalueParNom = evalueParNom,   // ← maintenant renseigné
                DateCreation = evaluation.DateCreation
            };
        }
    }
}
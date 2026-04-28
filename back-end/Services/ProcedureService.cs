// Services/ProcedureService.cs
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
    public class ProcedureService : IProcedureService
    {
        private readonly IProcedureRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IProcessusRepository _processusRepository;

        public ProcedureService(IProcedureRepository repository, IUserRepository userRepository, IProcessusRepository processusRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _processusRepository = processusRepository;
        }

        // ==================== PROCÉDURES ====================

        public async Task<ProcedureDto?> GetByIdAsync(Guid id)
        {
            var procedure = await _repository.GetByIdAsync(id);
            if (procedure == null) return null;
            return await MapToDto(procedure);
        }

        public async Task<IEnumerable<ProcedureDto>> GetAllAsync(Guid organisationId, string userRole, int userId)
        {
            var procedures = await _repository.GetAllAsync(organisationId, userRole, userId);
            var dtos = new List<ProcedureDto>();
            foreach (var p in procedures)
            {
                dtos.Add(await MapToDto(p));
            }
            return dtos;
        }

        public async Task<IEnumerable<ProcedureDto>> GetByProcessusAsync(Guid processusId, string userRole, int userId)
        {
            var procedures = await _repository.GetByProcessusAsync(processusId, userRole, userId);
            var dtos = new List<ProcedureDto>();
            foreach (var p in procedures)
            {
                dtos.Add(await MapToDto(p));
            }
            return dtos;
        }

        public async Task<IEnumerable<ProcedureDto>> GetFilteredAsync(Guid organisationId, string userRole, int userId, ProcedureFilterDto filter)
        {
            var filterParams = new ProcedureFilterParams
            {
                ProcessusId = filter.ProcessusId,
                SearchTerm = filter.SearchTerm,
                Statut = filter.Statut,
                ResponsableId = filter.ResponsableId
            };

            var procedures = await _repository.GetFilteredAsync(organisationId, userRole, userId, filterParams);
            var dtos = new List<ProcedureDto>();
            foreach (var p in procedures)
            {
                dtos.Add(await MapToDto(p));
            }

            // Pagination
            if (filter.Page > 0 && filter.PageSize > 0)
            {
                dtos = dtos.Skip((filter.Page - 1) * filter.PageSize)
                           .Take(filter.PageSize)
                           .ToList();
            }

            return dtos;
        }

        public async Task<ProcedureDto> CreateAsync(Guid organisationId, Guid processusId, CreateProcedureDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new ArgumentException("Le code de la procédure est requis");
            if (string.IsNullOrWhiteSpace(dto.Titre))
                throw new ArgumentException("Le titre de la procédure est requis");

            var codeExists = await _repository.CodeExistsAsync(dto.Code, organisationId);
            if (codeExists)
                throw new InvalidOperationException($"Une procédure avec le code {dto.Code} existe déjà");

            var responsable = await _userRepository.GetByIdAsync(dto.ResponsableId);
            if (responsable == null)
                throw new KeyNotFoundException("Responsable non trouvé");
            // ✅ Vérification du processus parent
            if (processusId == Guid.Empty)
                throw new ArgumentException("Le processus parent est requis.");
            var processus = await _processusRepository.GetByIdAsync(processusId);
            if (processus == null)
                throw new KeyNotFoundException("Processus parent non trouvé.");
            var procedure = new Procedure
            {
                OrganisationId = organisationId,
                ProcessusId = processusId,
                Code = dto.Code,
                Titre = dto.Titre,
                Objectif = dto.Objectif,
                DomaineApplication = dto.DomaineApplication,
                Description = dto.Description,
                ResponsableId = dto.ResponsableId,
                Statut = Enum.Parse<ProcedureStatut>(dto.Statut ?? "ACTIF", true)
            };

            var procedureId = await _repository.CreateAsync(procedure);
            var created = await _repository.GetByIdAsync(procedureId);

            // Ajouter les instructions
            if (dto.Instructions != null && dto.Instructions.Any())
            {
                int ordre = 1;
                foreach (var instrDto in dto.Instructions)
                {
                    var instruction = new Instruction
                    {
                        OrganisationId = organisationId,
                        ProcedureId = procedureId,
                        Code = instrDto.Code,
                        Titre = instrDto.Titre,
                        Description = instrDto.Description,
                        Ordre = instrDto.Ordre > 0 ? instrDto.Ordre : ordre++,
                        Statut = Enum.Parse<ProcedureStatut>(instrDto.Statut ?? "ACTIF", true)
                    };
                    await _repository.AddInstructionAsync(instruction);
                }
            }

            return await MapToDto(created);
        }

        public async Task<ProcedureDto> UpdateAsync(Guid id, UpdateProcedureDto dto)
        {
            var procedure = await _repository.GetByIdAsync(id);
            if (procedure == null)
                throw new KeyNotFoundException($"Procédure avec l'ID {id} non trouvée");

            if (!string.IsNullOrWhiteSpace(dto.Code) && dto.Code != procedure.Code)
            {
                var codeExists = await _repository.CodeExistsAsync(dto.Code, procedure.OrganisationId, id);
                if (codeExists)
                    throw new InvalidOperationException($"Une procédure avec le code {dto.Code} existe déjà");
                procedure.Code = dto.Code;
            }

            if (!string.IsNullOrWhiteSpace(dto.Titre)) procedure.Titre = dto.Titre;
            if (dto.Objectif != null) procedure.Objectif = dto.Objectif;
            if (dto.DomaineApplication != null) procedure.DomaineApplication = dto.DomaineApplication;
            if (dto.Description != null) procedure.Description = dto.Description;
            if (dto.ResponsableId.HasValue) procedure.ResponsableId = dto.ResponsableId.Value;
            if (!string.IsNullOrWhiteSpace(dto.Statut)) procedure.Statut = Enum.Parse<ProcedureStatut>(dto.Statut, true);

            var updated = await _repository.UpdateAsync(procedure);
            if (!updated) throw new Exception("Erreur lors de la mise à jour");

            // ← Supprimer et réinsérer les instructions
            if (dto.Instructions != null)
            {
                var existing = await _repository.GetInstructionsAsync(id);
                foreach (var ins in existing)
                    await _repository.DeleteInstructionAsync(ins.Id);

                int ordre = 1;
                foreach (var instrDto in dto.Instructions)
                {
                    var instruction = new Instruction
                    {
                        OrganisationId = procedure.OrganisationId,
                        ProcedureId = id,
                        Code = instrDto.Code,
                        Titre = instrDto.Titre,
                        Description = instrDto.Description,
                        Ordre = instrDto.Ordre > 0 ? instrDto.Ordre : ordre++,
                        Statut = Enum.Parse<ProcedureStatut>(instrDto.Statut ?? "ACTIF", true)
                    };
                    await _repository.AddInstructionAsync(instruction);
                }
            }

            var updatedProcedure = await _repository.GetByIdAsync(id);
            return await MapToDto(updatedProcedure);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new KeyNotFoundException($"Procédure avec l'ID {id} non trouvée");
            return await _repository.DeleteAsync(id);
        }

        // ==================== INSTRUCTIONS ====================

        public async Task<IEnumerable<InstructionDto>> GetInstructionsAsync(Guid procedureId)
        {
            var instructions = await _repository.GetInstructionsAsync(procedureId);
            return instructions.Select(i => new InstructionDto
            {
                Id = i.Id,
                Code = i.Code,
                Titre = i.Titre,
                Description = i.Description,
                Ordre = i.Ordre,
                Statut = i.Statut.ToString(),
                DateCreation = i.DateCreation
            });
        }

        public async Task<InstructionDto> AddInstructionAsync(Guid procedureId, CreateInstructionDto dto)
        {
            var procedure = await _repository.GetByIdAsync(procedureId);
            if (procedure == null)
                throw new KeyNotFoundException("Procédure non trouvée");

            var instruction = new Instruction
            {
                OrganisationId = procedure.OrganisationId,
                ProcedureId = procedureId,
                Code = dto.Code,
                Titre = dto.Titre,
                Description = dto.Description,
                Ordre = dto.Ordre,
                Statut = Enum.Parse<ProcedureStatut>(dto.Statut ?? "ACTIF", true)
            };

            var instructionId = await _repository.AddInstructionAsync(instruction);
            var created = await _repository.GetInstructionByIdAsync(instructionId);

            return new InstructionDto
            {
                Id = created.Id,
                Code = created.Code,
                Titre = created.Titre,
                Description = created.Description,
                Ordre = created.Ordre,
                Statut = created.Statut.ToString(),
                DateCreation = created.DateCreation
            };
        }

        public async Task<InstructionDto> UpdateInstructionAsync(Guid instructionId, UpdateInstructionDto dto)
        {
            var instruction = await _repository.GetInstructionByIdAsync(instructionId);
            if (instruction == null)
                throw new KeyNotFoundException("Instruction non trouvée");

            if (!string.IsNullOrWhiteSpace(dto.Code))
                instruction.Code = dto.Code;
            if (!string.IsNullOrWhiteSpace(dto.Titre))
                instruction.Titre = dto.Titre;
            if (dto.Description != null)
                instruction.Description = dto.Description;
            if (dto.Ordre.HasValue)
                instruction.Ordre = dto.Ordre.Value;
            if (!string.IsNullOrWhiteSpace(dto.Statut))
                instruction.Statut = Enum.Parse<ProcedureStatut>(dto.Statut, true);

            await _repository.UpdateInstructionAsync(instruction);

            return new InstructionDto
            {
                Id = instruction.Id,
                Code = instruction.Code,
                Titre = instruction.Titre,
                Description = instruction.Description,
                Ordre = instruction.Ordre,
                Statut = instruction.Statut.ToString(),
                DateCreation = instruction.DateCreation
            };
        }

        public async Task<bool> DeleteInstructionAsync(Guid instructionId)
        {
            return await _repository.DeleteInstructionAsync(instructionId);
        }

        // ==================== MAPPING ====================
        private async Task<ProcedureDto> MapToDto(Procedure procedure)
        {
            var instructions = await _repository.GetInstructionsAsync(procedure.Id);
            var responsable = await _userRepository.GetByIdAsync(procedure.ResponsableId);

            return new ProcedureDto
            {
                Id = procedure.Id,
                OrganisationId = procedure.OrganisationId,
                ProcessusId = procedure.ProcessusId,
                ResponsableId = procedure.ResponsableId,
                Code = procedure.Code,
                Titre = procedure.Titre,
                Objectif = procedure.Objectif,
                DomaineApplication = procedure.DomaineApplication,
                Description = procedure.Description,
                ProcessusCode = procedure.ProcessusCode,   // ← AJOUTER
                ProcessusNom = procedure.ProcessusNom,
                Responsable = responsable != null ? new ProcedureResponsableDto
                {
                    Id = responsable.Id,
                    NomComplet = $"{responsable.Prenom} {responsable.Nom}".Trim(),
                    Email = responsable.Email,
                    Fonction = responsable.Fonction ?? responsable.Role
                } : null,
                Statut = procedure.Statut.ToString(),
                InstructionsCount = instructions.Count(),
                DateCreation = procedure.DateCreation,
                DateModification = procedure.DateModification,
                Instructions = instructions.Select(i => new InstructionDto
                {
                    Id = i.Id,
                    Code = i.Code,
                    Titre = i.Titre,
                    Description = i.Description,
                    Ordre = i.Ordre,
                    Statut = i.Statut.ToString(),
                    DateCreation = i.DateCreation
                }).ToList(),
            };
        }
        public async Task<bool> CodeExistsAsync(string code, Guid organisationId, Guid? excludeId = null)
        {
            return await _repository.CodeExistsAsync(code, organisationId, excludeId);
        }
    }
}
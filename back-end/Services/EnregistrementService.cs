using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocApi.DTOs;
using DocApi.Domain;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace DocApi.Services
{
    public class EnregistrementService : IEnregistrementService
    {
        private readonly IEnregistrementRepository _repository;
        private readonly IWebHostEnvironment _env;

        public EnregistrementService(IEnregistrementRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        public async Task<IEnumerable<EnregistrementDto>> GetAllAsync(Guid? processusId, string userRole, int userId)
        {
            var enregs = await _repository.GetAllAsync(processusId, userRole, userId);
            return enregs.Select(e => new EnregistrementDto
            {
                Id = e.Id,
                ProcessusId = e.ProcessusId,
                ProcessusNom = e.ProcessusNom,
                ProcessusCode = e.ProcessusCode,
                TypeEnregistrement = e.TypeEnregistrement,
                Reference = e.Reference,
                Description = e.Description,
                FichierPath = e.FichierPath,
                DateEnregistrement = e.DateEnregistrement,
                CreeParNom = e.CreeParNom
            });
        }

        public async Task<EnregistrementDto> CreateAsync(CreateEnregistrementDto dto, int userId, Guid organisationId)
        {
            // 1. Sauvegarder le fichier
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Uploads", "Enregistrements");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{dto.Fichier.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Fichier.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("Uploads", "Enregistrements", fileName).Replace("\\", "/");
            var reference = await GenerateReference(organisationId);
            // 2. Créer l'entité
            var enreg = new Enregistrement
            {
                Id = Guid.NewGuid(),
                OrganisationId = organisationId,
                ProcessusId = dto.ProcessusId,
                TypeEnregistrement = dto.TypeEnregistrement,
                Reference = reference,
                Description = dto.Description,
                FichierPath = relativePath,
                DateEnregistrement = DateTime.UtcNow,
                CreeParId = userId
            };

            await _repository.CreateAsync(enreg);

            // 3. Retourner le DTO
            return new EnregistrementDto
            {
                Id = enreg.Id,
                ProcessusId = enreg.ProcessusId,
                TypeEnregistrement = enreg.TypeEnregistrement,
                Reference = enreg.Reference,
                Description = enreg.Description,
                FichierPath = enreg.FichierPath,
                DateEnregistrement = enreg.DateEnregistrement
            };
        }

        public async Task<(Stream stream, string contentType, string fileName)> GetFileAsync(Guid id)
        {
            var enreg = await _repository.GetByIdAsync(id);
            if (enreg == null)
                throw new FileNotFoundException("Enregistrement introuvable");

            var fullPath = Path.Combine(_env.ContentRootPath, enreg.FichierPath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Le fichier n'existe pas sur le serveur");

            var stream = File.OpenRead(fullPath);
            var contentType = GetContentType(fullPath);
            var fileName = Path.GetFileName(fullPath);
            return (stream, contentType, fileName);
        }

        public async Task DeleteAsync(Guid id)
        {
            var enreg = await _repository.GetByIdAsync(id);
            if (enreg != null)
            {
                var fullPath = Path.Combine(_env.ContentRootPath, enreg.FichierPath);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            await _repository.DeleteAsync(id);
        }


        private string GetContentType(string path) => Path.GetExtension(path)?.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
        private async Task<string> GenerateReference(Guid organisationId)
        {
            var currentYear = DateTime.UtcNow.Year;
            var prefix = $"ENREG-{currentYear}-";

            // Trouver la dernière référence de l'année pour cette organisation
            var lastRef = await _repository.GetLastReferenceAsync(organisationId, currentYear);
            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastRef))
            {
                var parts = lastRef.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                    nextNumber = lastNum + 1;
            }

            return $"{prefix}{nextNumber:D4}"; // D4 => 4 chiffres (0001)
        }
    }
}
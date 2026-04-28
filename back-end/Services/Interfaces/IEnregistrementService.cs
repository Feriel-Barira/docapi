using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocApi.DTOs;

namespace DocApi.Services.Interfaces
{
    public interface IEnregistrementService
    {
        Task<IEnumerable<EnregistrementDto>> GetAllAsync(Guid? processusId, string userRole, int userId);
        Task<EnregistrementDto> CreateAsync(CreateEnregistrementDto dto, int userId, Guid organisationId);
        Task<(Stream stream, string contentType, string fileName)> GetFileAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
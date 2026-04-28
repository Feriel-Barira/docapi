// Repositories/Interfaces/IUserRepository.cs
using DocApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);  // Ajouter ? pour nullable
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<IEnumerable<User>> GetAllAsync();
        Task<int> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string username, string email);
        Task<bool> ExistsAsync(int id);
    }
}
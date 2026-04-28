// back/Domain/Entities/User.cs
namespace DocApi.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";        
        public string? RoleGlobal { get; set; }                 
        public string? OrganisationId { get; set; }                 
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Fonction { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
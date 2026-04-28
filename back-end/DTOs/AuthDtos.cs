// back/DTOs/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace DocApi.DTOs
{
    public class LoginRequest
    {
        [Required]
        public required string UsernameOrEmail { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        public string Role { get; set; } = "User";
    }

    public class AuthResponse
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
        public string? Fonction { get; set; }
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? OrganisationId { get; set; }  
        public DateTime ExpiresAt { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public required string Token { get; set; }

        [Required]
        public required string RefreshToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public required string OldPassword { get; set; }

        [Required]
        [MinLength(6)]
        public required string NewPassword { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public string? OrganisationId { get; set; }  
        public string? Nom { get; set; }   
        public string? Prenom { get; set; }  
        public string? Fonction { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
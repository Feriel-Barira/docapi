// back/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DocApi.Common;
using DocApi.Domain.Entities;
using DocApi.DTOs;
using DocApi.Repositories.Interfaces;
using DocApi.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DocApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly JwtSettings _jwt;

        public AuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshRepo,
            IOptions<JwtSettings> jwt)
        {
            _userRepo = userRepo;
            _refreshRepo = refreshRepo;
            _jwt = jwt.Value;
        }

        // ── LOGIN ─────────────────────────────────────────────
        public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null)
        {
            var user = await _userRepo.GetByUsernameOrEmailAsync(request.UsernameOrEmail)
                       ?? throw new ServiceException("Identifiants incorrects.");

            if (!user.IsActive)
                throw new ServiceException("Compte désactivé.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new ServiceException("Identifiants incorrects.");

            var token = GenerateJwtToken(user.Id, user.Username, user.RoleGlobal ?? user.Role);
            var refreshToken = await CreateRefreshTokenAsync(user.Id, ipAddress);

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                Username = user.Username,
                Role = user.RoleGlobal ?? user.Role,
                OrganisationId = user.OrganisationId,
                Fonction = user.Fonction,
                Nom = user.Nom,
                Prenom = user.Prenom,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpirationInMinutes)
            };
        }

        // ── REGISTER ──────────────────────────────────────────
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepo.ExistsAsync(request.Username, request.Email))
                throw new ServiceException("Ce nom d'utilisateur ou email est déjà pris.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role ?? "User",
                RoleGlobal = "UTILISATEUR",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdId = await _userRepo.CreateAsync(user);
            var created = await _userRepo.GetByIdAsync(createdId)
                            ?? throw new ServiceException("Erreur création utilisateur.");

            var token = GenerateJwtToken(created.Id, created.Username, created.RoleGlobal ?? created.Role);
            var refresh = await CreateRefreshTokenAsync(created.Id, null);

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refresh.Token,
                Username = created.Username,
                Role = created.RoleGlobal ?? created.Role,
                OrganisationId = created.OrganisationId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpirationInMinutes)
            };
        }

        // ── REFRESH TOKEN ─────────────────────────────────────
        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null)
        {
            var stored = await _refreshRepo.GetByTokenAsync(request.RefreshToken)
                         ?? throw new ServiceException("Refresh token invalide.");

            if (stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
                throw new ServiceException("Refresh token expiré.");

            var user = await _userRepo.GetByIdAsync(stored.UserId)
                       ?? throw new ServiceException("Utilisateur introuvable.");

            await _refreshRepo.RevokeAsync(stored.Token);

            var newToken = GenerateJwtToken(user.Id, user.Username, user.RoleGlobal ?? user.Role);
            var newRefresh = await CreateRefreshTokenAsync(user.Id, ipAddress);

            return new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefresh.Token,
                Username = user.Username,
                Role = user.RoleGlobal ?? user.Role,
                OrganisationId = user.OrganisationId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpirationInMinutes)
            };
        }

        // ── LOGOUT ────────────────────────────────────────────
        public async Task LogoutAsync(string refreshToken)
        {
            var stored = await _refreshRepo.GetByTokenAsync(refreshToken);
            if (stored != null)
                await _refreshRepo.RevokeAsync(stored.Token);
        }

        // ── GET PROFILE ───────────────────────────────────────
        public async Task<UserResponse> GetUserProfileAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new NotFoundException("Utilisateur introuvable.");

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.RoleGlobal ?? user.Role,
                OrganisationId = user.OrganisationId,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        // ── CHANGE PASSWORD ───────────────────────────────────
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                throw new ServiceException("Ancien mot de passe incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            return await _userRepo.UpdateAsync(user);
        }

        // ── GENERATE JWT TOKEN ────────────────────────────────
        public string GenerateJwtToken(int userId, string username, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name,           username),
                new(ClaimTypes.Role,           role)
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ── HELPER ────────────────────────────────────────────
        private async Task<RefreshToken> CreateRefreshTokenAsync(int userId, string? ip)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationInDays),
                CreatedByIp = ip
            };
            await _refreshRepo.CreateAsync(token);
            return token;
        }
    }
}
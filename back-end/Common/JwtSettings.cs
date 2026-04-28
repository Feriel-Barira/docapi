namespace DocApi.Common
{
    public class JwtSettings
    {
        public required string SecretKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int ExpirationInMinutes { get; set; }
        // ← NOUVEAU : durée de vie du refresh token
        public int RefreshTokenExpirationInDays { get; set; } = 7;
    }
}
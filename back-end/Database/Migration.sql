-- =====================================================
-- Migration — À exécuter UNE SEULE FOIS sur DocDb
-- Ajoute les tables manquantes : RefreshTokens + AuditLogs
-- =====================================================

USE DocDb;

-- ── 1. RefreshTokens ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id          INT AUTO_INCREMENT PRIMARY KEY,
    UserId      INT NOT NULL,
    Token       VARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt   DATETIME     NOT NULL,
    IsRevoked   BOOLEAN      NOT NULL DEFAULT FALSE,
    CreatedAt   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedByIp VARCHAR(50)  NULL,

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_token   (Token(255)),
    INDEX idx_user    (UserId),
    INDEX idx_expires (ExpiresAt)
);

-- ── 2. AuditLogs ─────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id         INT AUTO_INCREMENT PRIMARY KEY,
    UserId     INT          NULL,
    Username   VARCHAR(100) NOT NULL DEFAULT 'Système',
    Action     VARCHAR(100) NOT NULL,   -- LOGIN, CREATE, UPDATE, DELETE, VALIDER ...
    EntityType VARCHAR(100) NOT NULL,   -- Processus, Document, NonConformite ...
    EntityId   VARCHAR(36)  NULL,
    Details    TEXT         NULL,
    IpAddress  VARCHAR(50)  NULL,
    DateAction DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_action     (Action),
    INDEX idx_entity     (EntityType, EntityId),
    INDEX idx_user       (UserId),
    INDEX idx_date       (DateAction)
);

-- ── 3. Nettoyage automatique des refresh tokens expirés ──────────────────────
SET GLOBAL event_scheduler = ON;

DROP EVENT IF EXISTS CleanExpiredRefreshTokens;
CREATE EVENT CleanExpiredRefreshTokens
    ON SCHEDULE EVERY 1 DAY
    DO DELETE FROM RefreshTokens WHERE ExpiresAt < NOW() OR IsRevoked = TRUE;

-- ── Vérification ─────────────────────────────────────────────────────────────
SELECT 'RefreshTokens créée' AS Resultat, COUNT(*) AS Lignes FROM RefreshTokens
UNION ALL
SELECT 'AuditLogs créée',                 COUNT(*)            FROM AuditLogs;

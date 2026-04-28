-- =====================================================
-- MIGRATION FIX — Exécuter dans phpMyAdmin
-- Aligne la table Users avec la documentation ISO 21001
-- =====================================================
USE DocDb;

-- ── ÉTAPE 1 : Ajouter OrganisationId dans Users ──────
-- La doc demande que chaque user appartienne à une organisation
ALTER TABLE Users 
ADD COLUMN IF NOT EXISTS OrganisationId CHAR(36) NULL,
ADD COLUMN IF NOT EXISTS Nom VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS Prenom VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS Fonction VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS RoleGlobal ENUM('ADMIN_ORG','UTILISATEUR','AUDITEUR','RESPONSABLE_SMQ') NOT NULL DEFAULT 'UTILISATEUR';

-- ── ÉTAPE 2 : Lier les users à l'organisation de démo ─
UPDATE Users SET 
    OrganisationId = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    RoleGlobal = CASE 
        WHEN Role = 'Admin' THEN 'ADMIN_ORG'
        ELSE 'UTILISATEUR'
    END,
    Nom = CASE Username
        WHEN 'admin'   THEN 'Mansouri'
        WHEN 'user1'   THEN 'Haddad'
        WHEN 'manager' THEN 'Mrad'
        ELSE Username
    END,
    Prenom = CASE Username
        WHEN 'admin'   THEN 'Ahmed'
        WHEN 'user1'   THEN 'Rania'
        WHEN 'manager' THEN 'Karim'
        ELSE ''
    END;

-- ── ÉTAPE 3 : Ajouter FK OrganisationId ──────────────
ALTER TABLE Users
ADD CONSTRAINT fk_users_organisation 
FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE SET NULL;

-- ── ÉTAPE 4 : Ajouter users de démo manquants ─────────
INSERT INTO Users (Username, Email, PasswordHash, Role, RoleGlobal, OrganisationId, Nom, Prenom, Fonction, IsActive)
VALUES
-- Responsable SMQ : smq123
('smq', 'smq@docapi.com',
 '$2a$11$gxHA2902.ZSkiq.ffzxOKefbk8vhYL4pbn.3VXUaPqRx7akHD1B/a',
 'User', 'RESPONSABLE_SMQ', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
 'Ben Ali', 'Sami', 'Responsable SMQ', TRUE),
-- Auditeur : audit123
('auditeur', 'auditeur@docapi.com',
 '$2a$11$kfJmjzRGyyStto7srRpIDuZV5FOSou5ho.ZkRez5LyWRziiwpI/T.',
 'User', 'AUDITEUR', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
 'Trabelsi', 'Leila', 'Auditeur Qualité', TRUE)
ON DUPLICATE KEY UPDATE Username = VALUES(Username);

-- ── Vérification ──────────────────────────────────────
SELECT Id, Username, Email, Role, RoleGlobal, OrganisationId, Nom, Prenom 
FROM Users;
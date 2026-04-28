-- =====================================================
-- ÉTAPE 1 : CreateDatabase_fixed.sql
-- Exécuter EN PREMIER dans phpMyAdmin
-- =====================================================

CREATE DATABASE IF NOT EXISTS DocDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE DocDb;

-- ── Users ────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    INDEX idx_username (Username),
    INDEX idx_email (Email),
    INDEX idx_role (Role)
);

-- ── Organisations ─────────────────────────────────────
CREATE TABLE IF NOT EXISTS Organisations (
    Id CHAR(36) PRIMARY KEY,
    Nom VARCHAR(100) NOT NULL,
    Code VARCHAR(20) NOT NULL UNIQUE,
    Description VARCHAR(500),
    Type ENUM('UNIVERSITE','INSTITUT','CENTRE','ENTREPRISE') NOT NULL,
    Adresse VARCHAR(200),
    Email VARCHAR(100),
    Telephone VARCHAR(20),
    Statut ENUM('ACTIF','SUSPENDUE') NOT NULL DEFAULT 'ACTIF',
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ── Processus ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS Processus (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    Code VARCHAR(20) NOT NULL,
    Nom VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Type ENUM('PILOTAGE','REALISATION','SUPPORT') NOT NULL,
    Finalites JSON DEFAULT ('[]'),
    Perimetres JSON DEFAULT ('[]'),
    Fournisseurs JSON DEFAULT ('[]'),
    Clients JSON DEFAULT ('[]'),
    DonneesEntree JSON DEFAULT ('[]'),
    DonneesSortie JSON DEFAULT ('[]'),
    Objectifs JSON DEFAULT ('[]'),
    PiloteId INT NOT NULL,
    Statut ENUM('ACTIF','INACTIF') NOT NULL DEFAULT 'ACTIF',
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateModification DATETIME NULL,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (PiloteId) REFERENCES Users(Id) ON DELETE RESTRICT,
    UNIQUE KEY unique_code_organisation (Code, OrganisationId)
);

-- ── ProcessusActeurs ──────────────────────────────────
CREATE TABLE IF NOT EXISTS ProcessusActeurs (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    ProcessusId CHAR(36) NOT NULL,
    UtilisateurId INT NOT NULL,
    TypeActeur ENUM('PILOTE','COPILOTE','CONTRIBUTEUR','OBSERVATEUR') NOT NULL,
    DateAffectation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProcessusId) REFERENCES Processus(Id) ON DELETE CASCADE,
    FOREIGN KEY (UtilisateurId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- ── Documents ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS Documents (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    ProcessusId CHAR(36) NULL,
    Code VARCHAR(20) NOT NULL,
    Titre VARCHAR(200) NOT NULL,
    TypeDocument ENUM('REFERENCE','TRAVAIL') NOT NULL,
    Description TEXT,
    Actif BOOLEAN DEFAULT TRUE,
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateModification DATETIME NULL,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProcessusId) REFERENCES Processus(Id) ON DELETE SET NULL,
    UNIQUE KEY unique_code_organisation (Code, OrganisationId)
);

-- ── VersionsDocuments ─────────────────────────────────
CREATE TABLE IF NOT EXISTS VersionsDocuments (
    Id CHAR(36) PRIMARY KEY,
    DocumentId CHAR(36) NOT NULL,
    OrganisationId CHAR(36) NOT NULL,
    NumeroVersion VARCHAR(10) NOT NULL,
    Statut ENUM('BROUILLON','EN_REVISION','VALIDE','OBSOLETE') NOT NULL,
    FichierPath VARCHAR(500),
    CommentaireRevision TEXT,
    EtabliParId INT NOT NULL,
    DateEtablissement DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    VerifieParId INT NULL,
    DateVerification DATETIME NULL,
    ValideParId INT NULL,
    DateValidation DATETIME NULL,
    DateMiseEnVigueur DATETIME NULL,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (EtabliParId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (VerifieParId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ValideParId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- ── Procedures ────────────────────────────────────────
CREATE TABLE IF NOT EXISTS Procedures (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    ProcessusId CHAR(36) NOT NULL,
    Code VARCHAR(20) NOT NULL,
    Titre VARCHAR(200) NOT NULL,
    Objectif VARCHAR(500),
    DomaineApplication VARCHAR(500),
    Description TEXT,
    ResponsableId INT NOT NULL,
    Statut ENUM('ACTIF','INACTIF') NOT NULL DEFAULT 'ACTIF',
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateModification DATETIME NULL,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProcessusId) REFERENCES Processus(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResponsableId) REFERENCES Users(Id) ON DELETE RESTRICT,
    UNIQUE KEY unique_code_organisation (Code, OrganisationId)
);

-- ── Instructions ──────────────────────────────────────
CREATE TABLE IF NOT EXISTS Instructions (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    ProcedureId CHAR(36) NOT NULL,
    Code VARCHAR(20) NOT NULL,
    Titre VARCHAR(200) NOT NULL,
    Description TEXT,
    Ordre INT DEFAULT 0,
    Statut ENUM('ACTIF','INACTIF') NOT NULL DEFAULT 'ACTIF',
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProcedureId) REFERENCES Procedures(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_code_procedure (Code, ProcedureId)
);

-- ── NonConformites ────────────────────────────────────
CREATE TABLE IF NOT EXISTS NonConformites (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    ProcessusId CHAR(36) NOT NULL,
    Reference VARCHAR(50) NOT NULL,
    DateDetection DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DetecteParId INT NOT NULL,
    Source ENUM('AUDIT','POINT_CONTROLE','RECLAMATION','AUTRE') NOT NULL,
    Gravite ENUM('MINEURE','MAJEURE','CRITIQUE') NOT NULL,
    Statut ENUM('OUVERTE','ANALYSE','ACTION_EN_COURS','CLOTUREE') NOT NULL DEFAULT 'OUVERTE',
    Description TEXT NOT NULL,
    ResponsableTraitementId INT NULL,
    DateCloture DATETIME NULL,
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateModification DATETIME NULL,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProcessusId) REFERENCES Processus(Id) ON DELETE CASCADE,
    FOREIGN KEY (DetecteParId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ResponsableTraitementId) REFERENCES Users(Id) ON DELETE SET NULL,
    UNIQUE KEY unique_reference_organisation (Reference, OrganisationId)
);

-- ── AnalysesCauses ────────────────────────────────────
CREATE TABLE IF NOT EXISTS AnalysesCauses (
    Id CHAR(36) PRIMARY KEY,
    NonConformiteId CHAR(36) NOT NULL,
    MethodeAnalyse ENUM('CINQ_M','ISHIKAWA','CINQ_POURQUOI','AUTRE') NOT NULL,
    Description TEXT NOT NULL,
    DateAnalyse DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AnalyseParId INT NOT NULL,
    FOREIGN KEY (NonConformiteId) REFERENCES NonConformites(Id) ON DELETE CASCADE,
    FOREIGN KEY (AnalyseParId) REFERENCES Users(Id) ON DELETE RESTRICT
);

-- ── HistoriqueNonConformites ──────────────────────────
CREATE TABLE IF NOT EXISTS HistoriqueNonConformites (
    Id CHAR(36) PRIMARY KEY,
    NonConformiteId CHAR(36) NOT NULL,
    AncienStatut ENUM('OUVERTE','ANALYSE','ACTION_EN_COURS','CLOTUREE') NOT NULL,
    NouveauStatut ENUM('OUVERTE','ANALYSE','ACTION_EN_COURS','CLOTUREE') NOT NULL,
    DateChangement DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ChangeParId INT NOT NULL,
    Commentaire TEXT,
    FOREIGN KEY (NonConformiteId) REFERENCES NonConformites(Id) ON DELETE CASCADE,
    FOREIGN KEY (ChangeParId) REFERENCES Users(Id) ON DELETE RESTRICT
);

-- ── ActionsCorrectives ────────────────────────────────
CREATE TABLE IF NOT EXISTS ActionsCorrectives (
    Id CHAR(36) PRIMARY KEY,
    NonConformiteId CHAR(36) NOT NULL,
    Type ENUM('CURATIVE','CORRECTIVE','PREVENTIVE') NOT NULL,
    Description TEXT NOT NULL,
    ResponsableId INT NOT NULL,
    DateEcheance DATETIME NOT NULL,
    Statut ENUM('PLANIFIEE','EN_COURS','REALISEE','VERIFIEE') NOT NULL DEFAULT 'PLANIFIEE',
    DateRealisation DATETIME NULL,
    CommentaireRealisation TEXT,
    VerifieParId INT NULL,
    DateVerification DATETIME NULL,
    CommentaireVerification TEXT,
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateModification DATETIME NULL,
    FOREIGN KEY (NonConformiteId) REFERENCES NonConformites(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResponsableId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (VerifieParId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_nonconformite (NonConformiteId),
    INDEX idx_statut (Statut)
);

-- ── Indicateurs ───────────────────────────────────────
CREATE TABLE IF NOT EXISTS Indicateurs (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    ProcessusId CHAR(36) NOT NULL,
    Code VARCHAR(20) NOT NULL,
    Nom VARCHAR(100) NOT NULL,
    Description TEXT,
    MethodeCalcul TEXT,
    Unite VARCHAR(20),
    ValeurCible DECIMAL(10,2),
    SeuilAlerte DECIMAL(10,2),
    FrequenceMesure ENUM('QUOTIDIEN','HEBDOMADAIRE','MENSUEL','TRIMESTRIEL','ANNUEL') NOT NULL,
    ResponsableId INT NOT NULL,
    Actif BOOLEAN DEFAULT TRUE,
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateModification DATETIME NULL,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProcessusId) REFERENCES Processus(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResponsableId) REFERENCES Users(Id) ON DELETE RESTRICT,
    UNIQUE KEY unique_code_organisation (Code, OrganisationId)
);

-- ── IndicateurValeurs ─────────────────────────────────
CREATE TABLE IF NOT EXISTS IndicateurValeurs (
    Id CHAR(36) PRIMARY KEY,
    IndicateurId CHAR(36) NOT NULL,
    Periode VARCHAR(20) NOT NULL,
    Valeur DECIMAL(10,2) NOT NULL,
    Commentaire TEXT,
    DateMesure DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    SaisiParId INT NOT NULL,
    FOREIGN KEY (IndicateurId) REFERENCES Indicateurs(Id) ON DELETE CASCADE,
    FOREIGN KEY (SaisiParId) REFERENCES Users(Id) ON DELETE RESTRICT,
    UNIQUE KEY unique_periode_indicateur (IndicateurId, Periode)
);

-- ── PointsControle (AUDITS) — NOUVEAU ────────────────
CREATE TABLE IF NOT EXISTS PointsControle (
    Id CHAR(36) PRIMARY KEY,
    OrganisationId CHAR(36) NOT NULL,
    ProcessusId CHAR(36) NOT NULL,
    Nom VARCHAR(200) NOT NULL,
    Description TEXT,
    Type ENUM('DOCUMENTAIRE','OPERATIONNEL','REGLEMENTAIRE','SYSTEME') NOT NULL,
    Frequence ENUM('QUOTIDIEN','HEBDOMADAIRE','MENSUEL','TRIMESTRIEL','SEMESTRIEL','ANNUEL') NOT NULL,
    ResponsableId INT NULL,
    Actif BOOLEAN DEFAULT TRUE,
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateModification DATETIME NULL,
    FOREIGN KEY (OrganisationId) REFERENCES Organisations(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProcessusId) REFERENCES Processus(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResponsableId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- ── EvaluationsPointControle (AUDITS) — NOUVEAU ──────
CREATE TABLE IF NOT EXISTS EvaluationsPointControle (
    Id CHAR(36) PRIMARY KEY,
    PointControleId CHAR(36) NOT NULL,
    DateEvaluation DATETIME NOT NULL,
    Conforme BOOLEAN NOT NULL DEFAULT TRUE,
    Commentaire TEXT,
    EvalueParId INT NULL,
    DateCreation DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PointControleId) REFERENCES PointsControle(Id) ON DELETE CASCADE,
    FOREIGN KEY (EvalueParId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- ── RefreshTokens ─────────────────────────────────────
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedByIp VARCHAR(50) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_token (Token(255)),
    INDEX idx_user (UserId)
);

-- ── AuditLogs ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,
    Username VARCHAR(100) NOT NULL DEFAULT 'Système',
    Action VARCHAR(100) NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    EntityId VARCHAR(36) NULL,
    Details TEXT NULL,
    IpAddress VARCHAR(50) NULL,
    DateAction DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_action (Action),
    INDEX idx_entity (EntityType, EntityId),
    INDEX idx_date (DateAction)
);

-- =====================================================
-- DONNÉES INITIALES
-- =====================================================

-- Users par défaut
INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt, IsActive)
VALUES
    ('admin', 'admin@docapi.com',
     '$2a$11$gxHA2902.ZSkiq.ffzxOKefbk8vhYL4pbn.3VXUaPqRx7akHD1B/a',
     'Admin', NOW(), TRUE),
    ('user1', 'user1@docapi.com',
     '$2a$11$kfJmjzRGyyStto7srRpIDuZV5FOSou5ho.ZkRez5LyWRziiwpI/T.',
     'User', NOW(), TRUE)
ON DUPLICATE KEY UPDATE Username = VALUES(Username);

-- Organisation de démonstration
INSERT INTO Organisations (Id, Nom, Code, Type, Statut, DateCreation)
VALUES (
    'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    'Mon Organisation',
    'ORG-001',
    'ENTREPRISE',
    'ACTIF',
    NOW()
) ON DUPLICATE KEY UPDATE Nom = VALUES(Nom);

-- Vérification finale
SELECT 'Users' AS Table_, COUNT(*) AS Total FROM Users
UNION ALL SELECT 'Organisations', COUNT(*) FROM Organisations
UNION ALL SELECT 'PointsControle', COUNT(*) FROM PointsControle
UNION ALL SELECT 'RefreshTokens', COUNT(*) FROM RefreshTokens
UNION ALL SELECT 'AuditLogs', COUNT(*) FROM AuditLogs;
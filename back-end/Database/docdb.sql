-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3307
-- Generation Time: Apr 28, 2026 at 01:29 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `docdb`
--

-- --------------------------------------------------------

--
-- Table structure for table `actionscorrectives`
--

CREATE TABLE `actionscorrectives` (
  `Id` char(36) NOT NULL,
  `NonConformiteId` char(36) NOT NULL,
  `Type` enum('CURATIVE','CORRECTIVE','PREVENTIVE') NOT NULL,
  `Description` text NOT NULL,
  `ResponsableId` int(11) NOT NULL,
  `DateEcheance` datetime NOT NULL,
  `Statut` enum('PLANIFIEE','EN_COURS','REALISEE','VERIFIEE') NOT NULL DEFAULT 'PLANIFIEE',
  `DateRealisation` datetime DEFAULT NULL,
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp(),
  `DateModification` datetime DEFAULT NULL,
  `preuveEnregistrementId` char(36) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `actionscorrectives`
--

INSERT INTO `actionscorrectives` (`Id`, `NonConformiteId`, `Type`, `Description`, `ResponsableId`, `DateEcheance`, `Statut`, `DateRealisation`, `DateCreation`, `DateModification`, `preuveEnregistrementId`) VALUES
('d82c0314-7b3f-4cc1-a886-b1061f22f140', '5128cb6a-0973-4640-86c0-18b60a2b1aef', 'CORRECTIVE', 'Mettre en place une checklist obligatoire avant validation.\n', 2, '2026-04-23 11:00:00', 'REALISEE', '2026-04-15 09:41:36', '2026-04-15 10:41:22', '2026-04-15 10:47:38', 'd9a26390-bc30-4d6f-9b69-b573100e301b');

-- --------------------------------------------------------

--
-- Table structure for table `analysescauses`
--

CREATE TABLE `analysescauses` (
  `Id` char(36) NOT NULL,
  `NonConformiteId` char(36) NOT NULL,
  `MethodeAnalyse` enum('CINQ_M','ISHIKAWA','CINQ_POURQUOI','AUTRE') NOT NULL,
  `Description` text NOT NULL,
  `DateAnalyse` datetime NOT NULL DEFAULT current_timestamp(),
  `AnalyseParId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `analysescauses`
--

INSERT INTO `analysescauses` (`Id`, `NonConformiteId`, `MethodeAnalyse`, `Description`, `DateAnalyse`, `AnalyseParId`) VALUES
('8118077f-ea88-4627-b2f8-369cf6e3ade7', '5128cb6a-0973-4640-86c0-18b60a2b1aef', 'CINQ_POURQUOI', ' absence de procédure de vérification des dossiers.\n', '2026-04-15 10:40:18', 3);

-- --------------------------------------------------------

--
-- Table structure for table `auditlogs`
--

CREATE TABLE `auditlogs` (
  `Id` int(11) NOT NULL,
  `UserId` int(11) DEFAULT NULL,
  `Username` varchar(100) NOT NULL DEFAULT 'Système',
  `Action` varchar(100) NOT NULL,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` varchar(36) DEFAULT NULL,
  `Details` text DEFAULT NULL,
  `IpAddress` varchar(50) DEFAULT NULL,
  `DateAction` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `auditlogs`
--

INSERT INTO `auditlogs` (`Id`, `UserId`, `Username`, `Action`, `EntityType`, `EntityId`, `Details`, `IpAddress`, `DateAction`) VALUES
(1, 3, 'admin', 'CREATE_PROCESSUS', 'Processus', '218c83d4-4e8f-4268-869c-73660ee372ba', 'Processus P-01 créé', '::1', '2026-04-14 22:14:37'),
(2, 3, 'admin', 'CREATE_PROCESSUS', 'Processus', '0cb45ec7-ee39-4429-b594-5720d69fd579', 'Processus P-02 créé', '::1', '2026-04-14 22:18:21'),
(3, 3, 'admin', 'CREATE_PROCEDURE', 'Procedure', 'ffaa22ec-9b9d-4184-85f1-1174991448cf', 'Procédure PRO-001 créée', '::1', '2026-04-14 22:24:20'),
(4, 3, 'admin', 'CREATE_PROCEDURE', 'Procedure', '475907bd-3438-450c-8fe9-634772004ee6', 'Procédure PRO-002 créée', '::1', '2026-04-15 08:36:46'),
(5, 3, 'admin', 'CREATE_DOCUMENT', 'Document', '27c182ec-bf04-455a-981e-64118409b8e9', 'Document MQ-001 créé', '::1', '2026-04-15 08:41:32'),
(6, 3, 'admin', 'UPDATE_PROCEDURE', 'Procedure', '475907bd-3438-450c-8fe9-634772004ee6', NULL, '::1', '2026-04-15 08:53:12'),
(7, 3, 'admin', 'UPDATE_PROCEDURE', 'Procedure', 'ffaa22ec-9b9d-4184-85f1-1174991448cf', NULL, '::1', '2026-04-15 09:03:44'),
(8, 3, 'admin', 'UPDATE_PROCEDURE', 'Procedure', '475907bd-3438-450c-8fe9-634772004ee6', NULL, '::1', '2026-04-15 09:04:34'),
(9, 3, 'admin', 'CREATE_DOCUMENT', 'Document', 'e886fef8-2d40-4d04-8912-8984934a2485', 'Document MQ-002 créé', '::1', '2026-04-15 09:06:32'),
(10, 3, 'admin', 'DELETE_DOCUMENT', 'Document', 'e886fef8-2d40-4d04-8912-8984934a2485', NULL, '::1', '2026-04-15 09:07:09'),
(11, 3, 'admin', 'CREATE_DOCUMENT', 'Document', 'd76bba87-8c68-407f-ae3e-c756bafbba47', 'Document MQ-002 créé', '::1', '2026-04-15 09:08:14'),
(12, 3, 'admin', 'CREATE_NC', 'NonConformite', 'feedce40-ce53-4efc-bd6f-2336c7fd25c5', 'NC 2026-P-01-001 créée', '::1', '2026-04-15 09:16:28'),
(13, 3, 'admin', 'CREATE_NC', 'NonConformite', '5128cb6a-0973-4640-86c0-18b60a2b1aef', 'NC 2026-P-02-002 créée', '::1', '2026-04-15 09:17:29'),
(14, 3, 'admin', 'CREATE_ENREGISTREMENT', 'Enregistrement', '81f70d07-e2cc-4c14-ad4a-1d0ef0edb478', 'Preuve ajoutée pour le processus 218c83d4-4e8f-4268-869c-73660ee372ba', '::1', '2026-04-15 09:19:53'),
(15, 3, 'admin', 'CREATE_ENREGISTREMENT', 'Enregistrement', '546f0e3e-5235-4c7f-ad9a-b2d7b1614de3', 'Preuve ajoutée pour le processus 0cb45ec7-ee39-4429-b594-5720d69fd579', '::1', '2026-04-15 09:39:19'),
(16, 3, 'admin', 'UPDATE_NC', 'NonConformite', '5128cb6a-0973-4640-86c0-18b60a2b1aef', 'Statut changé → ANALYSE', '::1', '2026-04-15 09:40:07'),
(17, 3, 'admin', 'UPDATE_NC', 'NonConformite', '5128cb6a-0973-4640-86c0-18b60a2b1aef', 'Statut changé → ACTION_EN_COURS', '::1', '2026-04-15 09:40:58'),
(18, 3, 'admin', 'CREATE_ENREGISTREMENT', 'Enregistrement', 'd9a26390-bc30-4d6f-9b69-b573100e301b', 'Preuve ajoutée pour le processus 0cb45ec7-ee39-4429-b594-5720d69fd579', '::1', '2026-04-15 09:47:38'),
(19, 3, 'admin', 'CREATE_USER', 'User', NULL, 'Utilisateur kbouaziz créé', '::1', '2026-04-15 11:14:30'),
(20, 3, 'admin', 'CREATE_USER', 'User', NULL, 'Utilisateur RNahla créé', '::1', '2026-04-15 12:57:29'),
(21, 3, 'admin', 'UPDATE_DOCUMENT', 'Document', 'e775da29-f1c4-4c8a-ac80-4d5067e319a5', 'Version archivée', '::1', '2026-04-16 20:27:28'),
(22, 3, 'admin', 'UPDATE_DOCUMENT', 'Document', 'e775da29-f1c4-4c8a-ac80-4d5067e319a5', 'Version archivée', '::1', '2026-04-16 20:27:28'),
(23, 3, 'admin', 'CREATE_NC', 'NonConformite', 'f0c95126-8945-4b5e-8824-bbac0e06a95c', 'NC 2026-P-02-003 créée', '::1', '2026-04-16 20:38:41'),
(24, 3, 'admin', 'UPDATE_NC', 'NonConformite', 'feedce40-ce53-4efc-bd6f-2336c7fd25c5', 'Statut changé → ANALYSE', '::1', '2026-04-16 21:03:10'),
(25, 3, 'admin', 'UPDATE_NC', 'NonConformite', 'feedce40-ce53-4efc-bd6f-2336c7fd25c5', 'Statut changé → ACTION_EN_COURS', '::1', '2026-04-16 21:03:43'),
(26, 3, 'admin', 'DELETE_NC', 'NonConformite', 'feedce40-ce53-4efc-bd6f-2336c7fd25c5', NULL, '::1', '2026-04-16 21:11:20'),
(27, 3, 'admin', 'CREATE_PROCESSUS', 'Processus', 'c863003a-fde0-421c-a061-1e0bd39f720e', 'Processus P-03 créé', '::1', '2026-04-22 17:25:42'),
(28, 3, 'admin', 'UPDATE_PROCESSUS', 'Processus', 'c863003a-fde0-421c-a061-1e0bd39f720e', 'Processus modifié', '::1', '2026-04-22 17:26:44'),
(29, 3, 'admin', 'CREATE_PROCEDURE', 'Procedure', '81836f6f-b847-4f4b-80fb-c8a618311e87', 'Procédure PRO-003 créée', '::1', '2026-04-24 17:56:29'),
(30, 3, 'admin', 'DELETE_PROCEDURE', 'Procedure', '81836f6f-b847-4f4b-80fb-c8a618311e87', NULL, '::1', '2026-04-24 18:39:46'),
(31, 3, 'admin', 'CREATE_PROCEDURE', 'Procedure', '692878da-903a-4bca-b466-22a030955746', 'Procédure pro-1 créée', '::1', '2026-04-24 19:46:23'),
(32, 3, 'admin', 'DELETE_PROCEDURE', 'Procedure', '692878da-903a-4bca-b466-22a030955746', NULL, '::1', '2026-04-24 19:46:26'),
(33, 3, 'admin', 'CREATE_ENREGISTREMENT', 'Enregistrement', 'eab682f7-8cec-4c0e-83ff-e1a0551bd8ca', 'Preuve ajoutée pour le processus c863003a-fde0-421c-a061-1e0bd39f720e', '::1', '2026-04-25 10:08:46'),
(34, 3, 'admin', 'DELETE_ENREGISTREMENT', 'Enregistrement', 'eab682f7-8cec-4c0e-83ff-e1a0551bd8ca', NULL, '::1', '2026-04-25 10:08:49'),
(35, 3, 'admin', 'UPDATE_USER', 'User', '1', NULL, '::1', '2026-04-25 10:10:25'),
(36, 3, 'admin', 'UPDATE_USER', 'User', '1', NULL, '::1', '2026-04-25 10:10:31'),
(37, 3, 'admin', 'DELETE_NC', 'NonConformite', 'f0c95126-8945-4b5e-8824-bbac0e06a95c', NULL, '::1', '2026-04-25 10:17:27'),
(38, 3, 'admin', 'CREATE_PROCESSUS', 'Processus', '58f5042b-4d0f-46bb-bbef-1b387a3193f8', 'Processus créé', '::1', '2026-04-26 09:37:42'),
(39, 3, 'admin', 'DELETE_PROCESSUS', 'Processus', '58f5042b-4d0f-46bb-bbef-1b387a3193f8', NULL, '::1', '2026-04-26 09:39:52'),
(40, 3, 'admin', 'CREATE_PROCEDURE', 'Procedure', '1a913712-985d-4b53-8bdb-24ed532fdc45', 'Procédure PRO-003 créée', '::1', '2026-04-26 09:40:13'),
(41, 3, 'admin', 'UPDATE_PROCEDURE', 'Procedure', '1a913712-985d-4b53-8bdb-24ed532fdc45', NULL, '::1', '2026-04-26 12:40:27'),
(42, 3, 'admin', 'CREATE_DOCUMENT', 'Document', 'f1e05152-b9b3-4fe9-b991-7279f1dfbe4e', 'Document mq-003 créé', '::1', '2026-04-26 17:58:45'),
(43, 3, 'admin', 'CREATE_NC', 'NonConformite', '8c6f289b-9c27-42ec-a202-ec109dc52d66', 'NC 2026-P-01-002 créée', '::1', '2026-04-26 18:48:58'),
(44, 3, 'admin', 'DELETE_NC', 'NonConformite', '8c6f289b-9c27-42ec-a202-ec109dc52d66', NULL, '::1', '2026-04-26 18:50:10'),
(45, 3, 'admin', 'CREATE_NC', 'NonConformite', '5a93b7ee-b184-4570-bbcd-652b160303c5', 'NC 2026-P-01-002 créée', '::1', '2026-04-26 18:50:27'),
(46, 3, 'admin', 'UPDATE_NC', 'NonConformite', '5a93b7ee-b184-4570-bbcd-652b160303c5', NULL, '::1', '2026-04-26 18:52:28'),
(47, 3, 'admin', 'UPDATE_NC', 'NonConformite', '5a93b7ee-b184-4570-bbcd-652b160303c5', NULL, '::1', '2026-04-26 18:53:21'),
(48, 3, 'admin', 'UPDATE_NC', 'NonConformite', '5a93b7ee-b184-4570-bbcd-652b160303c5', 'Statut changé → ANALYSE', '::1', '2026-04-26 18:53:26'),
(49, 3, 'admin', 'UPDATE_NC', 'NonConformite', '5a93b7ee-b184-4570-bbcd-652b160303c5', 'Statut changé → ACTION_EN_COURS', '::1', '2026-04-26 18:53:41'),
(50, 3, 'admin', 'CREATE_ENREGISTREMENT', 'Enregistrement', '2223b77c-1b69-4ed2-9b46-a7bb99bda7cf', 'Preuve ajoutée pour le processus c863003a-fde0-421c-a061-1e0bd39f720e', '::1', '2026-04-27 08:14:36'),
(51, 3, 'admin', 'DELETE_ENREGISTREMENT', 'Enregistrement', '2223b77c-1b69-4ed2-9b46-a7bb99bda7cf', NULL, '::1', '2026-04-27 08:36:16'),
(52, 653023, 'RNahla', 'DELETE_NC', 'NonConformite', '5a93b7ee-b184-4570-bbcd-652b160303c5', NULL, '::1', '2026-04-27 08:36:33'),
(53, 653023, 'RNahla', 'DELETE_DOCUMENT', 'Document', 'f1e05152-b9b3-4fe9-b991-7279f1dfbe4e', NULL, '::1', '2026-04-27 08:36:39'),
(54, 653023, 'RNahla', 'DELETE_PROCEDURE', 'Procedure', '1a913712-985d-4b53-8bdb-24ed532fdc45', NULL, '::1', '2026-04-27 08:36:46'),
(55, 3, 'admin', 'UPDATE_PROCESSUS', 'Processus', 'c863003a-fde0-421c-a061-1e0bd39f720e', 'Processus modifié', '::1', '2026-04-28 11:27:16');

-- --------------------------------------------------------

--
-- Table structure for table `documents`
--

CREATE TABLE `documents` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `ProcessusId` char(36) DEFAULT NULL,
  `Code` varchar(20) NOT NULL,
  `Titre` varchar(200) NOT NULL,
  `TypeDocument` enum('REFERENCE','TRAVAIL') NOT NULL,
  `Description` text DEFAULT NULL,
  `Actif` tinyint(1) DEFAULT 1,
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp(),
  `DateModification` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `documents`
--

INSERT INTO `documents` (`Id`, `OrganisationId`, `ProcessusId`, `Code`, `Titre`, `TypeDocument`, `Description`, `Actif`, `DateCreation`, `DateModification`) VALUES
('27c182ec-bf04-455a-981e-64118409b8e9', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', NULL, 'MQ-001', 'Politique qualité', 'REFERENCE', NULL, 1, '2026-04-15 09:41:32', NULL),
('d76bba87-8c68-407f-ae3e-c756bafbba47', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '0cb45ec7-ee39-4429-b594-5720d69fd579', 'MQ-002', 'Formulaire de demande de congé', 'TRAVAIL', NULL, 1, '2026-04-15 10:08:14', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `enregistrement`
--

CREATE TABLE `enregistrement` (
  `id` char(36) NOT NULL DEFAULT uuid(),
  `organisationid` char(36) NOT NULL,
  `processusid` char(36) NOT NULL,
  `typeenregistrement` varchar(50) NOT NULL DEFAULT 'PREUVE_EXECUTION',
  `reference` varchar(100) NOT NULL,
  `description` text DEFAULT NULL,
  `fichierpath` varchar(500) NOT NULL,
  `dateenregistrement` timestamp NOT NULL DEFAULT current_timestamp(),
  `creeparid` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `enregistrement`
--

INSERT INTO `enregistrement` (`id`, `organisationid`, `processusid`, `typeenregistrement`, `reference`, `description`, `fichierpath`, `dateenregistrement`, `creeparid`) VALUES
('546f0e3e-5235-4c7f-ad9a-b2d7b1614de3', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '0cb45ec7-ee39-4429-b594-5720d69fd579', 'PREUVE_EXECUTION', 'ENREG-2026-0002', 'Photo de l’étagère après réorganisation.\r\n', 'Uploads/Enregistrements/ced67d44-72d0-4197-ae12-349fb18144de_1.png', '2026-04-15 08:39:19', 3),
('81f70d07-e2cc-4c14-ad4a-1d0ef0edb478', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '218c83d4-4e8f-4268-869c-73660ee372ba', 'PREUVE_EXECUTION', 'ENREG-2026-0001', 'Compte rendu de la revue de direction', 'Uploads/Enregistrements/398d43ca-e9f8-4dd7-a1d5-c60254cb015e_preuve.pdf', '2026-04-15 08:19:53', 3),
('d9a26390-bc30-4d6f-9b69-b573100e301b', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '0cb45ec7-ee39-4429-b594-5720d69fd579', 'PREUVE_EXECUTION', 'ENREG-2026-0003', 'Mettre en place une checklist obligatoire avant validation.\r\n', 'Uploads/Enregistrements/eac785e1-2ea9-40bb-ad8e-8c202c8e3c87_MQ-001_v1.0.pdf', '2026-04-15 08:47:38', 3);

-- --------------------------------------------------------

--
-- Table structure for table `evaluationspointcontrole`
--

CREATE TABLE `evaluationspointcontrole` (
  `Id` char(36) NOT NULL,
  `PointControleId` char(36) NOT NULL,
  `DateEvaluation` datetime NOT NULL,
  `Conforme` tinyint(1) NOT NULL DEFAULT 1,
  `Commentaire` text DEFAULT NULL,
  `EvalueParId` int(11) DEFAULT NULL,
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `evaluationspointcontrole`
--

INSERT INTO `evaluationspointcontrole` (`Id`, `PointControleId`, `DateEvaluation`, `Conforme`, `Commentaire`, `EvalueParId`, `DateCreation`) VALUES
('3a29e4ab-7872-4449-bb9b-0ac8c09ce9ca', '44b64965-38d1-4b70-8351-18c5f93d9224', '2026-04-15 00:00:00', 1, 'hello', 4549, '2026-04-16 21:21:31'),
('3fbc24f0-f616-415c-9165-0a1ba63a4267', '1591845f-6789-4107-8a97-11764ce24e8d', '2026-04-12 00:00:00', 0, 'Deux dossiers manquent la pièce d’identité.', 1, '2026-04-15 09:34:33'),
('71e366eb-3811-465d-876e-c5f5ef923a17', '44b64965-38d1-4b70-8351-18c5f93d9224', '2026-04-10 00:00:00', 1, 'Tous les dossiers sont complets.', 2, '2026-04-15 09:33:58');

-- --------------------------------------------------------

--
-- Table structure for table `historiquenonconformites`
--

CREATE TABLE `historiquenonconformites` (
  `Id` char(36) NOT NULL,
  `NonConformiteId` char(36) NOT NULL,
  `AncienStatut` enum('OUVERTE','ANALYSE','ACTION_EN_COURS','CLOTUREE') NOT NULL,
  `NouveauStatut` enum('OUVERTE','ANALYSE','ACTION_EN_COURS','CLOTUREE') NOT NULL,
  `DateChangement` datetime NOT NULL DEFAULT current_timestamp(),
  `ChangeParId` int(11) NOT NULL,
  `Commentaire` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `historiquenonconformites`
--

INSERT INTO `historiquenonconformites` (`Id`, `NonConformiteId`, `AncienStatut`, `NouveauStatut`, `DateChangement`, `ChangeParId`, `Commentaire`) VALUES
('25c1a984-61c8-4a70-ac8b-f65b283b65c0', '5128cb6a-0973-4640-86c0-18b60a2b1aef', 'OUVERTE', 'ANALYSE', '2026-04-15 10:40:07', 3, NULL),
('492c0e2f-8c27-421c-9233-5723f2352dab', '5128cb6a-0973-4640-86c0-18b60a2b1aef', 'ANALYSE', 'ACTION_EN_COURS', '2026-04-15 10:40:58', 3, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `indicateurs`
--

CREATE TABLE `indicateurs` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `ProcessusId` varchar(36) DEFAULT NULL,
  `Code` varchar(20) NOT NULL,
  `Nom` varchar(100) NOT NULL,
  `Description` text DEFAULT NULL,
  `MethodeCalcul` text DEFAULT NULL,
  `Unite` varchar(20) DEFAULT NULL,
  `ValeurCible` decimal(10,2) DEFAULT NULL,
  `SeuilAlerte` decimal(10,2) DEFAULT NULL,
  `FrequenceMesure` enum('QUOTIDIEN','HEBDOMADAIRE','MENSUEL','TRIMESTRIEL','ANNUEL') NOT NULL,
  `ResponsableId` int(11) NOT NULL,
  `Actif` tinyint(1) DEFAULT 1,
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp(),
  `DateModification` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `indicateurs`
--

INSERT INTO `indicateurs` (`Id`, `OrganisationId`, `ProcessusId`, `Code`, `Nom`, `Description`, `MethodeCalcul`, `Unite`, `ValeurCible`, `SeuilAlerte`, `FrequenceMesure`, `ResponsableId`, `Actif`, `DateCreation`, `DateModification`) VALUES
('59a2c8ab-4ce8-422d-8c73-7ab8837a502c', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '0cb45ec7-ee39-4429-b594-5720d69fd579', 'IND-02', 'Délai moyen de traitement des congés', NULL, 'Somme des délais / Nombre de demandes', 'j', 3.00, 7.00, 'HEBDOMADAIRE', 4, 1, '2026-04-15 10:22:25', NULL),
('680eb762-9c09-4415-abe1-9de09d3c64fa', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '218c83d4-4e8f-4268-869c-73660ee372ba', 'IND-01', 'Taux d’inscription', NULL, '(Nombre d’inscrits / Capacité) × 100', '%', 95.00, 80.00, 'MENSUEL', 1, 1, '2026-04-15 10:21:11', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `indicateurvaleurs`
--

CREATE TABLE `indicateurvaleurs` (
  `Id` char(36) NOT NULL,
  `IndicateurId` char(36) NOT NULL,
  `Periode` varchar(20) NOT NULL,
  `Valeur` decimal(10,2) NOT NULL,
  `Commentaire` text DEFAULT NULL,
  `DateMesure` datetime NOT NULL DEFAULT current_timestamp(),
  `SaisiParId` int(11) NOT NULL,
  `OrganisationId` char(36) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `indicateurvaleurs`
--

INSERT INTO `indicateurvaleurs` (`Id`, `IndicateurId`, `Periode`, `Valeur`, `Commentaire`, `DateMesure`, `SaisiParId`, `OrganisationId`) VALUES
('602a6f1d-24e8-4232-b008-64dbc0ee73af', '680eb762-9c09-4415-abe1-9de09d3c64fa', 'Septembre 2026', 92.50, 'Baisse due à un problème technique sur le portail.\n', '2026-04-14 00:00:00', 1, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'),
('a60c8d03-f7a2-4cdf-b171-4e342c0b493b', '59a2c8ab-4ce8-422d-8c73-7ab8837a502c', 'Semaine 14', 4.20, 'Retard dû à l’absence d’un responsable.\n', '2026-04-13 00:00:00', 2, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee');

-- --------------------------------------------------------

--
-- Table structure for table `instructions`
--

CREATE TABLE `instructions` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `ProcedureId` char(36) NOT NULL,
  `Code` varchar(20) NOT NULL,
  `Titre` varchar(200) NOT NULL,
  `Description` text DEFAULT NULL,
  `Ordre` int(11) DEFAULT 0,
  `Statut` enum('ACTIF','INACTIF') NOT NULL DEFAULT 'ACTIF',
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `instructions`
--

INSERT INTO `instructions` (`Id`, `OrganisationId`, `ProcedureId`, `Code`, `Titre`, `Description`, `Ordre`, `Statut`, `DateCreation`) VALUES
('2bc373a9-a35a-4d71-9d95-885d9ba1a546', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'ffaa22ec-9b9d-4184-85f1-1174991448cf', 'INS-001', 'Remplir le formulaire d’inscription', 'L’étudiant saisit ses informations personnelles et choisit ses modules.', 1, 'ACTIF', '2026-04-15 10:03:44'),
('a3c646f9-7b2b-471d-ae45-3d383a338cd7', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '475907bd-3438-450c-8fe9-634772004ee6', 'INS-002', 'Soumettre la demande de congé', 'L’employé dépose la demande dans l’outil RH avant le 15 du mois.', 1, 'ACTIF', '2026-04-15 10:04:34');

-- --------------------------------------------------------

--
-- Table structure for table `nonconformites`
--

CREATE TABLE `nonconformites` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `ProcessusId` varchar(36) DEFAULT NULL,
  `Reference` varchar(50) NOT NULL,
  `DateDetection` datetime NOT NULL DEFAULT current_timestamp(),
  `DetecteParId` int(11) NOT NULL,
  `Source` enum('AUDIT','POINT_CONTROLE','RECLAMATION','AUTRE') NOT NULL,
  `Gravite` enum('MINEURE','MAJEURE','CRITIQUE') NOT NULL,
  `Statut` enum('OUVERTE','ANALYSE','ACTION_EN_COURS','CLOTUREE') NOT NULL DEFAULT 'OUVERTE',
  `Description` text NOT NULL,
  `ResponsableTraitementId` int(11) DEFAULT NULL,
  `DateCloture` datetime DEFAULT NULL,
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp(),
  `DateModification` datetime DEFAULT NULL,
  `Type` varchar(50) NOT NULL DEFAULT 'PRODUIT_SERVICE',
  `Nature` varchar(50) NOT NULL DEFAULT 'REELLE'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `nonconformites`
--

INSERT INTO `nonconformites` (`Id`, `OrganisationId`, `ProcessusId`, `Reference`, `DateDetection`, `DetecteParId`, `Source`, `Gravite`, `Statut`, `Description`, `ResponsableTraitementId`, `DateCloture`, `DateCreation`, `DateModification`, `Type`, `Nature`) VALUES
('5128cb6a-0973-4640-86c0-18b60a2b1aef', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '0cb45ec7-ee39-4429-b594-5720d69fd579', '2026-P-02-002', '2026-04-15 00:00:00', 3, 'RECLAMATION', 'CRITIQUE', 'ACTION_EN_COURS', 'Retard dans le paiement des salaires.\n', 2, NULL, '2026-04-15 10:17:29', '2026-04-15 10:40:58', 'PRODUIT_SERVICE', 'REELLE');

-- --------------------------------------------------------

--
-- Table structure for table `organisations`
--

CREATE TABLE `organisations` (
  `Id` char(36) NOT NULL,
  `Nom` varchar(100) NOT NULL,
  `Code` varchar(20) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Type` enum('UNIVERSITE','INSTITUT','CENTRE','ENTREPRISE') NOT NULL,
  `Adresse` varchar(200) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Telephone` varchar(20) DEFAULT NULL,
  `Statut` enum('ACTIF','SUSPENDUE') NOT NULL DEFAULT 'ACTIF',
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `organisations`
--

INSERT INTO `organisations` (`Id`, `Nom`, `Code`, `Description`, `Type`, `Adresse`, `Email`, `Telephone`, `Statut`, `DateCreation`) VALUES
('aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Mon Organisation', 'ORG-001', NULL, 'ENTREPRISE', NULL, NULL, NULL, 'ACTIF', '2026-04-09 16:32:33');

-- --------------------------------------------------------

--
-- Table structure for table `pointscontrole`
--

CREATE TABLE `pointscontrole` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `ProcessusId` varchar(36) DEFAULT NULL,
  `Nom` varchar(200) NOT NULL,
  `Description` text DEFAULT NULL,
  `Type` enum('DOCUMENTAIRE','OPERATIONNEL','REGLEMENTAIRE','SYSTEME') NOT NULL,
  `Frequence` enum('QUOTIDIEN','HEBDOMADAIRE','MENSUEL','TRIMESTRIEL','SEMESTRIEL','ANNUEL') NOT NULL,
  `ResponsableId` int(11) DEFAULT NULL,
  `Actif` tinyint(1) DEFAULT 1,
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp(),
  `DateModification` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `pointscontrole`
--

INSERT INTO `pointscontrole` (`Id`, `OrganisationId`, `ProcessusId`, `Nom`, `Description`, `Type`, `Frequence`, `ResponsableId`, `Actif`, `DateCreation`, `DateModification`) VALUES
('1591845f-6789-4107-8a97-11764ce24e8d', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '0cb45ec7-ee39-4429-b594-5720d69fd579', 'Contrôle des congés', 'Maintenant, lorsque vous ouvrez..', 'OPERATIONNEL', 'MENSUEL', 1, 1, '2026-04-15 09:32:55', '2026-04-16 21:19:38'),
('44b64965-38d1-4b70-8351-18c5f93d9224', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '218c83d4-4e8f-4268-869c-73660ee372ba', 'Vérification des dossiers étudiants', NULL, 'DOCUMENTAIRE', 'TRIMESTRIEL', 1, 1, '2026-04-15 09:32:18', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `procedures`
--

CREATE TABLE `procedures` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `ProcessusId` char(36) NOT NULL,
  `Code` varchar(20) NOT NULL,
  `Titre` varchar(200) NOT NULL,
  `Objectif` varchar(500) DEFAULT NULL,
  `DomaineApplication` varchar(500) DEFAULT NULL,
  `Description` text DEFAULT NULL,
  `ResponsableId` int(11) NOT NULL,
  `Statut` enum('ACTIF','INACTIF') NOT NULL DEFAULT 'ACTIF',
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp(),
  `DateModification` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `procedures`
--

INSERT INTO `procedures` (`Id`, `OrganisationId`, `ProcessusId`, `Code`, `Titre`, `Objectif`, `DomaineApplication`, `Description`, `ResponsableId`, `Statut`, `DateCreation`, `DateModification`) VALUES
('475907bd-3438-450c-8fe9-634772004ee6', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '0cb45ec7-ee39-4429-b594-5720d69fd579', 'PRO-002', 'Gestion des congés', 'Modalités de demande et validation des congés annuels.', '', '', 2, 'ACTIF', '2026-04-15 09:36:46', '2026-04-15 10:04:34'),
('ffaa22ec-9b9d-4184-85f1-1174991448cf', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '218c83d4-4e8f-4268-869c-73660ee372ba', 'PRO-001', 'Procédure d’inscription en ligne', 'Décrire les étapes pour que l’étudiant puisse s’inscrire via le portail web.', 'Tous les étudiants en licence/master.', '', 1, 'ACTIF', '2026-04-14 23:24:20', '2026-04-15 10:03:44');

-- --------------------------------------------------------

--
-- Table structure for table `processus`
--

CREATE TABLE `processus` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `Code` varchar(20) NOT NULL,
  `Nom` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Type` enum('PILOTAGE','REALISATION','SUPPORT') NOT NULL,
  `Finalites` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT '[]' CHECK (json_valid(`Finalites`)),
  `Perimetres` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT '[]' CHECK (json_valid(`Perimetres`)),
  `Fournisseurs` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT '[]' CHECK (json_valid(`Fournisseurs`)),
  `Clients` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT '[]' CHECK (json_valid(`Clients`)),
  `DonneesEntree` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT '[]' CHECK (json_valid(`DonneesEntree`)),
  `DonneesSortie` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT '[]' CHECK (json_valid(`DonneesSortie`)),
  `Objectifs` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT '[]' CHECK (json_valid(`Objectifs`)),
  `PiloteId` int(11) NOT NULL,
  `Statut` enum('ACTIF','INACTIF') NOT NULL DEFAULT 'ACTIF',
  `DateCreation` datetime NOT NULL DEFAULT current_timestamp(),
  `DateModification` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `processus`
--

INSERT INTO `processus` (`Id`, `OrganisationId`, `Code`, `Nom`, `Description`, `Type`, `Finalites`, `Perimetres`, `Fournisseurs`, `Clients`, `DonneesEntree`, `DonneesSortie`, `Objectifs`, `PiloteId`, `Statut`, `DateCreation`, `DateModification`) VALUES
('0cb45ec7-ee39-4429-b594-5720d69fd579', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'P-02', 'Gestion des ressources humaines', ' Administration du personnel, paie, formation.', 'SUPPORT', '[]', '[]', '[]', '[]', '[\"Demande de recrutement\"]', '[\"Contrat de travail\",\"Fiche de paie\"]', '[]', 2, 'ACTIF', '2026-04-14 23:18:21', NULL),
('218c83d4-4e8f-4268-869c-73660ee372ba', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'P-01', 'Gestion des inscriptions', 'Processus d’inscription des étudiants pour l’année universitaire.', 'REALISATION', '[\"Garantir l\\u2019acc\\u00E8s aux formations\",\"Optimiser le taux de remplissage\"]', '[\"D\\u00E9partement p\\u00E9dagogique\",\"Service scolarit\\u00E9\"]', '[]', '[]', '[]', '[]', '[]', 1, 'ACTIF', '2026-04-14 23:14:37', NULL),
('c863003a-fde0-421c-a061-1e0bd39f720e', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'P-03', 'Évaluation des acquis des apprenants', 'Ce processus couvre la conception, l’organisation, la correction et l’analyse des évaluations (examens, QCM, mises en situation) pour mesurer l’atteinte des objectifs pédagogiques.', 'PILOTAGE', '[]', '[]', '[]', '[]', '[]', '[]', '[]', 4549, 'ACTIF', '2026-04-22 18:25:42', '2026-04-28 12:27:16');

-- --------------------------------------------------------

--
-- Table structure for table `processusacteurs`
--

CREATE TABLE `processusacteurs` (
  `Id` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `ProcessusId` char(36) NOT NULL,
  `UtilisateurId` int(11) NOT NULL,
  `TypeActeur` enum('PILOTE','COPILOTE','CONTRIBUTEUR','OBSERVATEUR') NOT NULL,
  `DateAffectation` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `processusacteurs`
--

INSERT INTO `processusacteurs` (`Id`, `OrganisationId`, `ProcessusId`, `UtilisateurId`, `TypeActeur`, `DateAffectation`) VALUES
('c41cac06-4c56-4a5a-b7ef-31016c2811b9', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'c863003a-fde0-421c-a061-1e0bd39f720e', 2, 'CONTRIBUTEUR', '2026-04-28 12:27:16');

-- --------------------------------------------------------

--
-- Table structure for table `refreshtokens`
--

CREATE TABLE `refreshtokens` (
  `Id` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Token` varchar(500) NOT NULL,
  `ExpiresAt` datetime NOT NULL,
  `IsRevoked` tinyint(1) NOT NULL DEFAULT 0,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `CreatedByIp` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `refreshtokens`
--

INSERT INTO `refreshtokens` (`Id`, `UserId`, `Token`, `ExpiresAt`, `IsRevoked`, `CreatedAt`, `CreatedByIp`) VALUES
(1, 3, 'Txsjdq5P6snqdtf9wa+JuRV0AlPEkcyLSIW4ANvYv3uIhwTxFWvpmsGq9sHXDMWEl5isb/zt03G97vYON74A+A==', '2026-04-16 16:28:20', 0, '2026-04-09 16:28:20', '::1'),
(2, 3, 'AJj731hWI+HE9Oepdc2nS1KELgAsWo+TGtrLoDeZ5piD9+iWfkY1ggN3mlQk0PuLVcIzxcFdo/IEFdLGm/XFXA==', '2026-04-16 17:03:59', 0, '2026-04-09 17:03:59', '::1'),
(3, 3, 'we0vxXnd1AjOykx7gmhC06lTa/g+7tCcPvjr7+m7N0CPVG53oRJuvCqFuPqOy5j0ZJmB6F+3FIZAlrt3IBZmUg==', '2026-04-17 11:55:21', 0, '2026-04-10 11:55:21', '::1'),
(4, 3, 'BV/Z0D/dbfpAFpVjpQA7id9TlsbAgCOhGndPxIzEHgyL4SIrm9n8EFAw6Gf3Nj1592EjQpCNMB/7lJHfgdgUMQ==', '2026-04-17 12:18:20', 0, '2026-04-10 12:18:20', '::1'),
(5, 3, 'u2udXfcFgG2X4+Y7oQR7g3ia1Tl5ataYJy9ibS6PD1HI0n3/liDCDMkbWkhzsjeABQlFfC/HXKOXJQHDde8NGA==', '2026-04-17 12:20:28', 0, '2026-04-10 12:20:28', '::1'),
(6, 3, 'rN5VkG1Z9w09lumXOtXHHnRQvpBi9nlAjN1ZFniDoV9tc16nMcIQ+mMg5jOianIrE03/8u49B/tWKT+RKvOpWQ==', '2026-04-17 13:23:59', 0, '2026-04-10 13:23:59', '::1'),
(7, 3, 'g02uqxwA62Te8Scdv4EyhOJhr9vRKCzcqu7/1f3k29FZLmuhlXH1ptRxj/YD+3G1+LFV7GeDvzHIU3uqoTOHsA==', '2026-04-17 13:35:15', 0, '2026-04-10 13:35:15', '::1'),
(8, 3, 'hRqMZx/oqWxf2E46fc1Vzx9u/Y1/cf9tNSomP0siUTecjPKJioYCn2pSWUPoGKlwVUK1rvPgO/mq9Z6Uw7giiA==', '2026-04-17 13:38:38', 0, '2026-04-10 13:38:38', '::1'),
(9, 3, '/SRE+bASmkst7qhPKT9FI5qxRPm1NR1HftiVB9vTVg6vL60/fd6SngNTe6CoyIEL4U2MIDXX3qRxUeUfOcQ6Mg==', '2026-04-17 16:01:32', 0, '2026-04-10 16:01:32', '::1'),
(11, 3, 'UtbaBXIT80uyybQleIa1j90lgSEWidxV7+Jeg15XGkpBS8VaGO3hYFNcMsIf3NCdjWp72Xym1rqzo0rXh40uqw==', '2026-04-18 13:30:34', 0, '2026-04-11 13:30:34', '::1'),
(12, 3, 'uBEqXpK4x5zkqFIXUV9m575ubVbaeORNLpH1+jtyxMo93ZSdX5knOryVoZau9oHDBME8UmN25Tw9qdaslptK9A==', '2026-04-19 13:45:34', 0, '2026-04-12 13:45:34', '::1'),
(13, 3, '6wsgjISuaKTRYX648dV+TlcAfJrS0CEAEQ0+pFDm/KAMwVbvlVJvafkaUnTMgKptIyMyHhLB0xVrEcb+PSGR7A==', '2026-04-19 14:20:46', 0, '2026-04-12 14:20:46', '::1'),
(14, 3, 'LIl2zAKXRufV88h2CayzveY1LwIOm+zr2JVOy+xX7QECfEiUyWHccnIY3/FQMv+IANpIHPg8T6qPiSjcr/pvbA==', '2026-04-19 20:23:42', 0, '2026-04-12 20:23:42', '::1'),
(15, 3, 'ovFtMiJhVPMQfFuuQBibJeGoObs49eLG/Acff7onQP7LkPTQxVhmDb/pFVjb4+fYVxnL2vDDsDnDbMX70RVeuw==', '2026-04-19 20:25:24', 0, '2026-04-12 20:25:24', '::1'),
(16, 3, 'YxxHbtSMXorWJndWI58ess1xPz4veEiCXGVYEgXcX1cNvZhSpscM9jizI3Kg9t7LWTWLjDMdx232NzlJPIYeQA==', '2026-04-19 20:29:09', 0, '2026-04-12 20:29:09', '::1'),
(17, 3, 'tXlrETh3HptY7N6KxKJeL+d6bW80MvMnz7aPWOgcBmbUlS3OSiET10+Rhlf+Q2L/eisLipYpwWuCVcdtMrT46Q==', '2026-04-19 20:32:29', 0, '2026-04-12 20:32:29', '::1'),
(18, 3, '0/SQl1SAVw+NW3Fi4JCLsLz7zQrdF/oHOkB1068TwGUI6/JnvMl2M8I+SBc+mhMN1BFBdE2DRJBm5AAbmmH8KQ==', '2026-04-19 20:42:07', 0, '2026-04-12 20:42:07', '::1'),
(19, 3, '7hDo41aOGwgOATFZGzZlOdz/H5PKHHYKAfvvJ+r+ERkvyY6M3BZxHMRCGkg5FD80WEF2PFq54v3xgGao7H0U1g==', '2026-04-20 15:38:37', 0, '2026-04-13 15:38:37', '::1'),
(20, 3, '+UyMytCEgKaImMuvcsEwhUZURLRgWY9ScuN5Qd65Ekpfk+PcKrz1kvkQJIlBTckr4NmSNEm2cDdHqM1zmkirGw==', '2026-04-20 19:24:14', 0, '2026-04-13 19:24:14', '::1'),
(21, 3, 'T5wuCy0kEt/uB/Sqfi1nIhlMPla/N11yNTj+nxbGdr4VwtTcv1Rm08Oc+4jIuPZuyZoalcN3T9kEDZCcarjIDQ==', '2026-04-21 13:16:36', 0, '2026-04-14 13:16:36', '::1'),
(22, 4, 'sfFPNYgfXEim9pqmyJqcOWNpJgm9NBr+/BGUUpMcR9Hd02RgUpbhTowLpl2wtDOY3s+T+IlcigY5xD1LIaVxyg==', '2026-04-22 11:26:22', 0, '2026-04-15 11:26:22', '::1'),
(23, 3, 'yqmPNuL0XqrHZJ20GGIL2TYR+/OodM+NSQkezM7sJzwaJMPUweMJA9wavVdo0rF+fBQ/QHVBFefn69u+irAdyQ==', '2026-04-22 11:26:53', 0, '2026-04-15 11:26:53', '::1'),
(24, 4, '+MDZWTocANN6rOmf9n7691/yWm7kPSnDxUULqp6Z8aLL5d1243SPpE8j9kgHk7xBHl+GdL/AI9T2SE28m61i4A==', '2026-04-22 12:51:36', 0, '2026-04-15 12:51:36', '::1'),
(25, 4549, 'cHFrJIuY/J4tfXVLcAdCrQ1lwDhdKIZ1zfgKUpomuoWWTFKBBMHzrkNtK8NarZxNi7PbhgUVfoYsKzvazsg8Ww==', '2026-04-22 12:53:33', 0, '2026-04-15 12:53:33', '::1'),
(26, 653023, 'RfKKHI/PfrnFpidrn1/pu6eOgeDFV9Ne0jAEfJSndPsDoNEEYGVbQtJddCs3oFZodlU4c+nAtW6qTij9UsslIA==', '2026-04-22 12:57:42', 0, '2026-04-15 12:57:42', '::1'),
(27, 4549, 'LIq4oN40bexmQaSODbL2GbqhUytGnjzHPsfFEnXBB7r42/rZ+oFkzfsztLhMOv/X0IHt5AZkFXxDnQCAcARh8A==', '2026-04-22 13:17:22', 0, '2026-04-15 13:17:22', '::1'),
(28, 4, 'gC/gN+UcghPBYIt1SaTl5q2Y6E+7SqIVzaAbBgzWsTZghhMOvcUQeLhOs9CIxKvqEuD6PGUlD6IbJZJp3IF3Dw==', '2026-04-22 13:18:29', 0, '2026-04-15 13:18:29', '::1'),
(29, 653023, '/CXQR8Z2pAxlMMKyuP4Wo8hmVRcg70LO0s4ukhdQBhTfZx1A3kcFFtXrNitQLsXwJroblu4yaDalkUn6Akp36A==', '2026-04-22 13:19:08', 0, '2026-04-15 13:19:08', '::1'),
(30, 4, 's3pDNYNhEjvNqWwNf7U/8rJDvyaq37qe++Tdz22zWCVxwiWkkTPM5HlPSllfqlmIgS3SwzPlteZHU1e5Yqzu5A==', '2026-04-22 14:55:06', 0, '2026-04-15 14:55:06', '::1'),
(31, 4549, '4iW48ITUESNedyD+WtaarCNKqsvYu9hZGZm6GE/QXpuWyxjj8//V0JrHAMd9NW65LDpMPl6G3zKfl69xbyvcgQ==', '2026-04-22 14:55:32', 0, '2026-04-15 14:55:32', '::1'),
(32, 4, 'L+bAo9s11w3OuA2GEeRJeykEXvVWdTQaGEZ2STM7i/7YDVrgkubU1YiEVFgdet9Jh2x7+lZG3U9a0zx+WZKWcg==', '2026-04-22 15:50:32', 0, '2026-04-15 15:50:32', '::1'),
(33, 4549, 'cTHRNbhB3RzGmdHZ+l7o8kPBpVOqSolcLqvA3qQq9Ndd+J5bntRVOo5u1wXYwllHxWHvTyz99ZWLc2FdSztgJA==', '2026-04-22 16:22:30', 0, '2026-04-15 16:22:30', '::1'),
(34, 653023, 'zwaVpQfpct6Qt8Y4Yj6OzuYqIeocZEUOIeRF6AL5hCUxZz5yiVMUAFVSdPMoBCfhlRn62APFBOZISLQtSxOrOA==', '2026-04-22 16:23:26', 0, '2026-04-15 16:23:26', '::1'),
(35, 4549, 'CC+cyLJ/WohOp8XhiqcrV2xRxCM1qFjdatXwkzUevT4eVWRtxaTGgq4nTfJVzGnw6T2A64IZCz/WGQYza7zbpA==', '2026-04-22 16:36:07', 0, '2026-04-15 16:36:07', '::1'),
(36, 4, 'lk12g9OTbVZ7up/cxJ0rsp0kiJXkQQfDBPFwq781eYB9x9E8kUMz2i97dRvElYt7K7wqZ+WZaj/texym3URJHg==', '2026-04-22 16:37:34', 0, '2026-04-15 16:37:34', '::1'),
(37, 4549, 'U1uggE6dSiE5MWh9Uun2mOmUulbtii73Jpoo++nIWX2u8KY4WOHDeRXWik1fTLUgsOPZez4EbkIqSsiLXUoWAA==', '2026-04-22 18:34:09', 0, '2026-04-15 18:34:09', '::1'),
(38, 653023, 'rg6V4JP2xyphQacjBfWocyRaHSjDxsLLnvWct4qnA0XFiRTcBlIbDNYSpWSW2WZDLu2f5rYToh/ewy8prKN1bw==', '2026-04-22 22:05:02', 0, '2026-04-15 22:05:02', '::1'),
(39, 4, 'nGAEOz+li0YaWeCUZCYl9MxHg1ChPFc4NGEHqlo0E4hOngKww7nwVdjRt2ZRjLGKA5flfRtePw8lujsWVqFrOA==', '2026-04-22 22:05:56', 0, '2026-04-15 22:05:56', '::1'),
(40, 4, 'EZn97gHYHev9MsLJnyZN6wgGblRcRcm7FCov1YaVEsyiu18gH8zfFTvT6qo8GJro3OdUKWgNOa2e5kcEw+J94g==', '2026-04-22 23:18:05', 0, '2026-04-15 23:18:05', '::1'),
(41, 653023, 'iJjOqb1MxULVI54wv5ivBuG8APo97dit0S3zpgDrmhTYZuKlJSMd+8eFPiaZO3EBWHox5j4uq4uf4gcIDhtj/Q==', '2026-04-22 23:20:07', 0, '2026-04-15 23:20:07', '::1'),
(42, 4549, 'hPiCLq01NOBSUuSYZ3qk4xj+Vm1mbukao8JvTpIafL2jrRIsEiUN7b4NPqBhB4aMOSh6A2jSKvcnpx2ECyMD/A==', '2026-04-22 23:20:37', 0, '2026-04-15 23:20:37', '::1'),
(43, 653023, '9DFQHIU2zHW9lnJ3c7Qbv1kyWwJNG3XtINcimO1/93uwS1TrvBZ0e/vKHBgn4Z86IqLFXZBBYq7lf6rL8B15wg==', '2026-04-22 23:22:27', 0, '2026-04-15 23:22:27', '::1'),
(44, 4, 'at0KZr6ic9VGzPJnig6NzIMNcKD9B5hGSAp+/AEaEFquFRGJhPfcFHEGgo696qX1OYeSY4ViJOZfQPqXgh+7Ww==', '2026-04-22 23:57:12', 0, '2026-04-15 23:57:12', '::1'),
(45, 4549, 'MaYcmUPYgR9sPvql4IHEIbFWCUYbRg1MQRRgYy3MZ33MWESF2dH02zT2KRc8N8Z2+3fgists+fyYA+SJDTy4lA==', '2026-04-22 23:57:51', 0, '2026-04-15 23:57:51', '::1'),
(46, 653023, 'xXt2n83ovnr2cOOm5JOf8ckfvBxt8Tp5s+YX6E8UJ5cedvlq8WRFuXXrM2fDJtUULKwVNtfDOF09FgJndZfQPw==', '2026-04-22 23:58:03', 0, '2026-04-15 23:58:03', '::1'),
(47, 4, '+Hk8rYNfrK/3JzCOWSgAivInYaX13ypFL2zYS3dG04yMydIS8/rI5WutJBqrClh4eO3QEWmKgAihS+LF7GI4ng==', '2026-04-23 00:13:44', 0, '2026-04-16 00:13:44', '::1'),
(48, 4549, '8aZ5LmPKSaVK4+C7SWCsCooOoI9VKV3vZlwizYVPAeCkXsVGKDxhctIn0e80PMBIACRUAOMoMCq9UpJOFD+dZg==', '2026-04-23 00:14:18', 0, '2026-04-16 00:14:18', '::1'),
(49, 653023, 'Hghy54/yUsRSZVVPj6ffH9x+T2VkuojiL/lVPexOtsCnoN6aR8Fkb3V6/skLwazHzZWvf6NBR/kpQJ3IrlPSTg==', '2026-04-23 00:14:34', 0, '2026-04-16 00:14:34', '::1'),
(50, 4549, '2FItEqMWi6qHr6I/xF0Rm23G1ijjmzPHMMsrelRGDczynU5+L+m7DbKUcvqlxumqgzTVZpOIpimPtRfvBJ9A8A==', '2026-04-23 00:15:37', 0, '2026-04-16 00:15:37', '::1'),
(51, 4, 'aIgwyw+KfN6MX9Y+CQqclslmiBYSsYdSSEy1z5DYGtetNmpcBJYXvYJjdBHtuMp/j/bEsiKBkMHfHwwE9jAkWw==', '2026-04-23 00:25:29', 0, '2026-04-16 00:25:29', '::1'),
(52, 4549, 'BNIaRE3ZWJDRQb0QOkXYmFEE6wfdJSsCv9zDAyFx5K/lG86ZbU+997LRSCbDfMIWSfzH7NOZYuS5UVTNNeww5Q==', '2026-04-23 00:25:51', 0, '2026-04-16 00:25:51', '::1'),
(53, 653023, 'q2hSkKfji33mUxDzB+HpMwweS3nMIJ35DLYXu1UGMnqAthi5hPI2QrQt3xErFoG+F/39CuztNct/ld9zwNVzGw==', '2026-04-23 00:26:42', 0, '2026-04-16 00:26:42', '::1'),
(54, 4, 'sgdl672ifBXaeO20+y79sg12+VsuOxhbuudD8PWF0GlzeY5Sm4i1t4E9LRkxUUud3RveRnFtDe+zKy5isRw9oA==', '2026-04-23 00:44:00', 0, '2026-04-16 00:44:00', '::1'),
(55, 4549, 'wyC3DK4bDpcpr0beYJ1SNmIgUCcbiExl2uByV/SiRbKA3GQWB1ahqYVx6lKmknf5NbAf0jvD1yeHtWD49TQXAg==', '2026-04-23 00:44:17', 0, '2026-04-16 00:44:17', '::1'),
(56, 4, 'wrpMSxHDbZeFfStj0dsfxxy0C2TytHkMQDgONOK5UA6UKmD4iSvh6/3Mf+LBi0jny5A69qaOxVFRNKh2nrDmDw==', '2026-04-23 00:44:35', 0, '2026-04-16 00:44:35', '::1'),
(57, 653023, 'ZyE/eofdbp64j5fP/YpExjfToAsDDt3RrHFMVN1zSI2no2GiUFM+dhbKOUmbwTZ8jeNdPuRSxW4J/chKbN4Qvg==', '2026-04-23 00:44:51', 0, '2026-04-16 00:44:51', '::1'),
(58, 4549, 'zf9gMXgygGeZb6/fllMN2sFGRTARxGPBFTOe+zbagUhlQRalg1tRi573DtfcsceVWxxhQLu2B8sUZFeiunSu8w==', '2026-04-23 00:45:05', 0, '2026-04-16 00:45:05', '::1'),
(59, 4, 'uPoDISDU0et9NmVaFHGKM9ySm8UU3p2qm/wTLc6h7IrQLbdFqj1dEBSmLWclo6Pxa49XSyuNH/Jj/jKJzCnV1g==', '2026-04-23 00:46:26', 0, '2026-04-16 00:46:26', '::1'),
(60, 4549, 'Y0sBzC4CCVxLOKvltECZ+HICTPNKcyRXx11aWy7zTLnGLawgpG5y1smKKMbd327ISHVBmBJkVKWvN7Fnl1SxwQ==', '2026-04-23 00:46:41', 0, '2026-04-16 00:46:41', '::1'),
(61, 653023, 'Yy9xw34PKn3/jVRWoSj+1uvHmA4jVOPkMRUNUJdEfue3mxPbVy07Ayj82SWdWTdw1KlzXgIOsJLHF+0ZfINELA==', '2026-04-23 00:46:54', 0, '2026-04-16 00:46:54', '::1'),
(62, 3, '75Ho7Hfb2bXBXgyPyBku3NTFKC+YJtIwJipsQsUncHmDG8KvY+46pISyVxN0+zI7GDTCKUsZIM+aCJA/KK8pOA==', '2026-04-23 20:03:14', 0, '2026-04-16 20:03:14', '::1'),
(63, 3, 'a7Wv58wvVkumrxZLYLMoL9c8p7Ar0VpkUiIi8Fq2JINl3sJJyiy6wMYF0+XJsMA8zdogtuTzEVA8bks9ZsfO4w==', '2026-04-23 22:00:55', 0, '2026-04-16 22:00:55', '::1'),
(64, 3, 'cL2lMgnOrSI2nUF7o9ihVTgvOqQfuibxCvWhbmDTnxyK3R73XdGO93fHgsChiQeTh51muUXclZ5BSI00pF8zYQ==', '2026-04-28 09:41:48', 0, '2026-04-21 09:41:48', '::1'),
(65, 3, '6FvSk40i3hZGEEnbizvaY/2Mp6sUqHnzOV1LZW01MfKGszdajYrOtrPFrR6/evxp8vwk+YWO+BBHqep4LAOdqA==', '2026-04-28 09:59:47', 0, '2026-04-21 09:59:47', '::1'),
(66, 3, '+szx+ozSpW3ehltLcZNsEMjzW8WiXsWkzrr5lrb4hw6adaBljamzlwH92qo0i2U8RFhX/HDIoJxMd516ZZ2Jmw==', '2026-04-28 10:05:14', 0, '2026-04-21 10:05:14', '::1'),
(67, 4549, 'y1rEXp/HKi6PFFuR+TNQ9bOTVJUKpKF53RkwgLovOKvy40oV61ZvPJlVLzO1fxScixbhPE6/PQONCU9lUId+RQ==', '2026-04-28 10:05:42', 0, '2026-04-21 10:05:42', '::1'),
(68, 3, 'plcQdTsO5lnY3/kuEQq0WfeZ5FpPSs0JrW3C7wnPP7DEnF4JUW/2qHuNz9ZmWjyDYYQYJGzJ2hhGfOGlUTrTKw==', '2026-04-28 10:06:10', 0, '2026-04-21 10:06:10', '::1'),
(69, 3, 'biOWA50Ypx1CSlMpkWYXvEL4XcKJwQzPBfe8gcU33qk5DALE18nhwGh8O7R61e4Pe8pZVZbr2h9+sQxlfZAdMg==', '2026-04-28 10:15:59', 0, '2026-04-21 10:15:59', '::1'),
(70, 4549, 'A9OU9TBffFyuWla93g/tyyk402CAtIVVxDmwsIfuxDGqRMBLTjUrb0yI64AKcWdd+sIdDdiobo6FzuzjD8QsWQ==', '2026-04-28 10:16:20', 0, '2026-04-21 10:16:20', '::1'),
(71, 653023, 'J4yM07LQvtklisZWuz8hpDnNapFEVmx/KWKHkUKwGIeRPBZLwAEqbuIuBxvHnJFi7D2a+YEUdeaIi+G6FM3+6g==', '2026-04-28 10:41:15', 0, '2026-04-21 10:41:15', '::1'),
(72, 3, 'ZJXMuUpOQ98GCYETD0plVEKFhTCpT4pHcaiFIOJ/6rDRMBrLqXbVd7CqGdtyfu56w4e9c6cz0bWBhpbwPJR0uQ==', '2026-04-29 15:53:14', 0, '2026-04-22 15:53:14', '::1'),
(73, 3, 'qeuLSgQOVOik+NPGhC44xSWKsEdFUeYyer3LXu1bkDrSCPAxiXXqnxAf8KAZR6bYFhUsgZGgBRpA4RU2gtIY/g==', '2026-05-01 17:22:36', 0, '2026-04-24 17:22:36', '::1'),
(74, 3, 'CQxhl4i76sW47HDEcvNhr9TRU+81G3S9vJiLAoUR/Y7TlBTz1QrBluVxpsEwsGqMp/7qaV2hs4XqbGnYPD1R2g==', '2026-05-02 10:03:57', 0, '2026-04-25 10:03:57', '::1'),
(75, 3, 'WrLMenANQoqs6OINrso3J2hVa/TmysBg1ZVGOvP9Y79gS2IyVXS/LD3OTgqisUAy8JjykKSdzSgWZsMXdHpCGg==', '2026-05-02 11:38:06', 0, '2026-04-25 11:38:06', '::1'),
(76, 4549, 'YqGu4K4glgOn0tCpB3Sw5x/LjMD2PuDowA1/7TMUeiSNkpfD9JaMC+hbgEkAxQplOyY4em+1uN363BZDm1rgHQ==', '2026-05-02 19:05:39', 0, '2026-04-25 19:05:39', '::1'),
(77, 4549, 'xb0+4JvM/h5PlVibEbVHAlYM1MZEDe7CgFILAWk2d+tPS1ZNY1gVAJ3BwYF+vH8FAswes1tq8wlU0V3RxkYFfg==', '2026-05-02 20:35:18', 0, '2026-04-25 20:35:18', '::1'),
(78, 4, 'DNs2fcnBwKoIvM23oBJL0lHeQOP0gHHpnCzk0jfaPqrSib0E60TCwaJJiCtetFIbpGO7T9Jz+E4v3F4aNvuWkw==', '2026-05-02 20:35:43', 0, '2026-04-25 20:35:43', '::1'),
(79, 4549, 'pSjW87UroNXucNe+uNLtPZ0Mz/9ZSx86oGL+c185YW98in+P3tq2GtXqlUJHL90CIOMRMzSLIq6oE9uTCSviQQ==', '2026-05-02 20:37:16', 0, '2026-04-25 20:37:16', '::1'),
(80, 653023, 'aF1RIoqxIbd/cdoCfZmgG0gSjnMtenDRJe9ytZ43Hycuanue2eeyAAEqgbBWxljBbAMcYaDNRs7EHzyLo+wR3w==', '2026-05-02 20:37:36', 0, '2026-04-25 20:37:36', '::1'),
(81, 4549, 'P++rn92aG2K/rUWBnukcVn14qkn6tXa6dQ7H4DVU2dGtyNU73kMkgtVIzFAWtVdpSJNTWAFxhcPoRxFoyv1gAg==', '2026-05-02 20:39:19', 0, '2026-04-25 20:39:19', '::1'),
(82, 4549, 'isGOYVGYJ7m7bq8v7+si+R+mwIuVyaN2Y8DDBXR6Tq9hYm6ZAhnpOz24B19dGzxCfCqxdPmp0jFiGa3jbghqMA==', '2026-05-03 09:36:26', 0, '2026-04-26 09:36:26', '::1'),
(83, 3, 'e4NSS9UbN9fh1MQlpoakzh1NBKuEul81G9V95wU8Twa7GRDwypM0J4jpnCDUd/jnUb/ROI+0O++eTwy0dvbr/g==', '2026-05-03 09:37:06', 0, '2026-04-26 09:37:06', '::1'),
(84, 4549, 'hIGHyv9QSssU8GUdB5X9IuDMkzsoz/PmMeZoL/W5HTHFYWIHKrSADjNKzPLvl9XR6kIsJkkryMFZcT8YZrcdvA==', '2026-05-03 10:35:50', 0, '2026-04-26 10:35:50', '::1'),
(85, 4549, 'V5Q7Q7PY1Ya1IRT0iQ45Fxvz1ay+Rl4ihdMudmFS9z1goLvrsqlD6vVVFNTd6/a7HPZXWcSd713O8bUZqIp3Ow==', '2026-05-03 11:26:50', 0, '2026-04-26 11:26:50', '::1'),
(86, 3, 'nIJhbazUx86gQ+83bV8swzF/WzQN7i3j5ktej1jHoDXkUUc2jaKg8HIJ6m7QdDpK1oLmywUWxrHIOBAqQV7cWg==', '2026-05-03 12:57:30', 0, '2026-04-26 12:57:30', '::1'),
(87, 4549, 'bZIXCTMwak49X61g574orP9BPMKM4gCHHun+KzDjpO5UO/p8cDQ9wXxDZnoz/rv3BBYFTVXEU9RSjSms2hUp9A==', '2026-05-03 12:57:47', 0, '2026-04-26 12:57:47', '::1'),
(88, 3, 'k8MfkhxMjyCh6DHp//XIPMRYBz6vcNiRbwKZ51ocYThVt95phmtff4d6WNoClKMQhnu9m/C1e96FXqotMUJRIg==', '2026-05-03 15:46:01', 0, '2026-04-26 15:46:01', '::1'),
(89, 4549, 'DGk/rI0uw0m4R0+FcnssJrHNaBtvR7Kg4fXKYFPhrPfcG0NdBCVsOvrXGKp60S81THfC5prVU9UsUJT9k1/sbQ==', '2026-05-03 17:16:08', 0, '2026-04-26 17:16:08', '::1'),
(90, 4549, 'EOUIDO2ag82T3YL3nrZi5MAu1xUaYPtiRmdAu5m+ccLaqjqDrXD4xSNyLFBuDzkhTLaHGsTH3x/3iuxwR2zWXA==', '2026-05-03 17:59:03', 0, '2026-04-26 17:59:03', '::1'),
(91, 4549, 'j4ApWTAjx7m9+cuvU2z0woKYtnWXuGx1/6AOEUSoTpylvTXcWyFeIGjqweHJbYsmSWIlCRnUzjNf9jDhvvBXRQ==', '2026-05-03 18:49:14', 0, '2026-04-26 18:49:14', '::1'),
(92, 4549, 'WsFT9gdLsRtfTpKGvlulNdapR/cXlIRQ1g5UNnLaqLXJf4A5jdGzFjLM2ja6wazAEQL46J4PpAoStvmSKg/RNw==', '2026-05-03 18:54:19', 0, '2026-04-26 18:54:19', '::1'),
(93, 3, 'WhWoqx+ASdR8GSJSYVeNziyDO+qANBiYEv6FOr1uzYVbRI606IxQOEu4xyDgHhIZwsOXwWSDSj1HynSVC6A3ig==', '2026-05-04 08:14:11', 0, '2026-04-27 08:14:11', '::1'),
(94, 653023, '4NwtEJu8ZYnt6tgPRjIfoTjsziHEgrHK6HzaXeUqETrcL97G7pxD1m8e71Km7QUYzx4kyDecHHtpZzIv3XeqkQ==', '2026-05-04 08:15:05', 0, '2026-04-27 08:15:05', '::1'),
(95, 4, 'FkDw8P7ht3CzBzB/oBwyMHzOiaUDmEQRz7jbs2gYDKxMOrn35l8NCLZxCqguT9UpDXdH3PcHCXZxNvXDR85/vQ==', '2026-05-04 08:18:34', 0, '2026-04-27 08:18:34', '::1'),
(96, 653023, '+Oknm+RTPWclKAZCN6OydByNXBaeogsf/FpUsOzXzwd+1EUoE+m9+OHdy/f91PF5TaobjFYtRaDoHOewrz++jw==', '2026-05-04 08:35:48', 0, '2026-04-27 08:35:48', '::1'),
(97, 3, 'Dg1P7ojfr0l6DHfZ9UwEwAEGwJ5exZiRaeK7ntHJm6okUtzUXOktp9MoBbpAFMAOgQ0KdMzjGg+sUDN8tBcZ6A==', '2026-05-04 08:48:22', 0, '2026-04-27 08:48:22', '::1'),
(98, 3, '8zwoqz2qwEWbrOomqURCknssiEm1Fb/8QBheJ4qV+NxVnfRJ5R2s7XqLoeHG1IjphX85ihxvmv+z5WkAZAfWmg==', '2026-05-04 09:29:57', 0, '2026-04-27 09:29:57', '::1'),
(99, 4549, 'wz+hOcVAZgoA4Mvb5rCPc2gVVP5k1bPk+p7brvXj7PMJVJHZBPtLy2a2c0qVE/nz6N+D9sNx2b2aPy504h7Cyw==', '2026-05-04 09:30:15', 0, '2026-04-27 09:30:15', '::1'),
(100, 3, 'jcDKjBu9Ygz4MyV0C8OfpKsBmFr6NM8uqExCDu+MNKG01etCxvjiFTh4en+ADCK8W8iMXgdtBnrzxh/3CYshsA==', '2026-05-04 14:16:31', 0, '2026-04-27 14:16:31', '::1');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `Id` int(11) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Role` varchar(20) NOT NULL DEFAULT 'User',
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `OrganisationId` char(36) DEFAULT NULL,
  `Nom` varchar(100) DEFAULT NULL,
  `Prenom` varchar(100) DEFAULT NULL,
  `Fonction` varchar(100) DEFAULT NULL,
  `RoleGlobal` enum('ADMIN_ORG','UTILISATEUR','AUDITEUR','RESPONSABLE_SMQ') NOT NULL DEFAULT 'UTILISATEUR'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`Id`, `Username`, `Email`, `PasswordHash`, `Role`, `CreatedAt`, `IsActive`, `OrganisationId`, `Nom`, `Prenom`, `Fonction`, `RoleGlobal`) VALUES
(1, 'smq', 'smq@docapi.com', '$2a$11$gxHA2902.ZSkiq.ffzxOKefbk8vhYL4pbn.3VXUaPqRx7akHD1B/a', 'User', '2026-04-09 16:19:43', 1, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Ben Ali', 'Sami', 'Responsable SMQ', 'RESPONSABLE_SMQ'),
(2, 'auditeur', 'auditeur@docapi.com', '$2a$11$kfJmjzRGyyStto7srRpIDuZV5FOSou5ho.ZkRez5LyWRziiwpI/T.', 'User', '2026-04-09 16:19:43', 1, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Trabelsi', 'Leila', 'Auditeur Qualité', 'AUDITEUR'),
(3, 'admin', 'admin@docapi.com', '$2a$11$gxHA2902.ZSkiq.ffzxOKefbk8vhYL4pbn.3VXUaPqRx7akHD1B/a', 'Admin', '2026-04-09 16:29:30', 1, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Mansouri', 'Ahmed', 'Admin', 'ADMIN_ORG'),
(4, 'user', 'user@docapi.com', '$2a$11$kfJmjzRGyyStto7srRpIDuZV5FOSou5ho.ZkRez5LyWRziiwpI/T.', 'User', '2026-04-09 16:29:30', 1, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Haddad', 'Rania', 'User', 'UTILISATEUR'),
(4549, 'kbouaziz', 'karim.bouaziz@docapi.com', '$2a$11$pw7WeI9KTkySr2T0FyeSbO1S9qBqEkSw3a1Nn3GV78EtD/w9mJgja', 'User', '2026-04-15 12:14:30', 1, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Bouaziz', 'Karim', 'Auditeur Interne', 'AUDITEUR'),
(653023, 'RNahla', 'Nahla@docapi.com', '$2a$11$DVdPDuVoVVdEJF7mE4Gw0ueH4a0xHg5Q1uyDJzMW8cuKh5/WI74Py', 'User', '2026-04-15 13:57:29', 1, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'Rebha', 'Nahla', 'Responsable smq', 'RESPONSABLE_SMQ');

-- --------------------------------------------------------

--
-- Table structure for table `versionsdocuments`
--

CREATE TABLE `versionsdocuments` (
  `Id` char(36) NOT NULL,
  `DocumentId` char(36) NOT NULL,
  `OrganisationId` char(36) NOT NULL,
  `NumeroVersion` varchar(10) NOT NULL,
  `Statut` enum('BROUILLON','EN_REVISION','VALIDE','OBSOLETE') NOT NULL,
  `FichierPath` varchar(500) DEFAULT NULL,
  `CommentaireRevision` text DEFAULT NULL,
  `EtabliParId` int(11) NOT NULL,
  `DateEtablissement` datetime NOT NULL DEFAULT current_timestamp(),
  `VerifieParId` int(11) DEFAULT NULL,
  `DateVerification` datetime DEFAULT NULL,
  `ValideParId` int(11) DEFAULT NULL,
  `DateValidation` datetime DEFAULT NULL,
  `DateMiseEnVigueur` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `versionsdocuments`
--

INSERT INTO `versionsdocuments` (`Id`, `DocumentId`, `OrganisationId`, `NumeroVersion`, `Statut`, `FichierPath`, `CommentaireRevision`, `EtabliParId`, `DateEtablissement`, `VerifieParId`, `DateVerification`, `ValideParId`, `DateValidation`, `DateMiseEnVigueur`) VALUES
('36e3e9ba-ce3e-4641-a5e8-2ed4cf55d08b', 'd76bba87-8c68-407f-ae3e-c756bafbba47', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '2.0', 'BROUILLON', 'document-pdf-exemple.pdf', '', 2, '2026-04-07 00:00:00', 2, '2026-04-08 00:00:00', 2, '2026-04-09 00:00:00', '2026-04-10 00:00:00'),
('6f90a2b6-cbb8-44fc-a41b-df2065aa1c58', 'd76bba87-8c68-407f-ae3e-c756bafbba47', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '2.1', 'EN_REVISION', 'preuve.pdf', '', 3, '2026-04-08 00:00:00', 3, '2026-04-09 00:00:00', 3, '2026-04-10 00:00:00', '2026-04-17 00:00:00'),
('8c6c052e-e2e9-4736-8d5c-88daa5cdeb22', '27c182ec-bf04-455a-981e-64118409b8e9', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '1.0', 'BROUILLON', 'exemple-1.pdf', '', 1, '2026-04-15 00:00:00', 1, '2026-04-16 00:00:00', 1, '2026-04-17 00:00:00', '2026-04-18 00:00:00'),
('e775da29-f1c4-4c8a-ac80-4d5067e319a5', 'd76bba87-8c68-407f-ae3e-c756bafbba47', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', '2.2', 'OBSOLETE', 'document-pdf-exemple.pdf', '', 3, '2026-04-03 00:00:00', 4, '2026-04-11 00:00:00', 4549, '2026-04-21 00:00:00', '2026-04-30 00:00:00');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `actionscorrectives`
--
ALTER TABLE `actionscorrectives`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `ResponsableId` (`ResponsableId`),
  ADD KEY `idx_nonconformite` (`NonConformiteId`),
  ADD KEY `idx_statut` (`Statut`),
  ADD KEY `preuveEnregistrementId` (`preuveEnregistrementId`);

--
-- Indexes for table `analysescauses`
--
ALTER TABLE `analysescauses`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `NonConformiteId` (`NonConformiteId`),
  ADD KEY `AnalyseParId` (`AnalyseParId`);

--
-- Indexes for table `auditlogs`
--
ALTER TABLE `auditlogs`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `UserId` (`UserId`),
  ADD KEY `idx_action` (`Action`),
  ADD KEY `idx_entity` (`EntityType`,`EntityId`),
  ADD KEY `idx_date` (`DateAction`);

--
-- Indexes for table `documents`
--
ALTER TABLE `documents`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_code_organisation` (`Code`,`OrganisationId`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `ProcessusId` (`ProcessusId`);

--
-- Indexes for table `enregistrement`
--
ALTER TABLE `enregistrement`
  ADD PRIMARY KEY (`id`),
  ADD KEY `organisationid` (`organisationid`),
  ADD KEY `processusid` (`processusid`),
  ADD KEY `creeparid` (`creeparid`);

--
-- Indexes for table `evaluationspointcontrole`
--
ALTER TABLE `evaluationspointcontrole`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PointControleId` (`PointControleId`),
  ADD KEY `EvalueParId` (`EvalueParId`);

--
-- Indexes for table `historiquenonconformites`
--
ALTER TABLE `historiquenonconformites`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `NonConformiteId` (`NonConformiteId`),
  ADD KEY `ChangeParId` (`ChangeParId`);

--
-- Indexes for table `indicateurs`
--
ALTER TABLE `indicateurs`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_code_organisation` (`Code`,`OrganisationId`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `ResponsableId` (`ResponsableId`),
  ADD KEY `ProcessusId` (`ProcessusId`);

--
-- Indexes for table `indicateurvaleurs`
--
ALTER TABLE `indicateurvaleurs`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_periode_indicateur` (`IndicateurId`,`Periode`),
  ADD KEY `SaisiParId` (`SaisiParId`);

--
-- Indexes for table `instructions`
--
ALTER TABLE `instructions`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_code_procedure` (`Code`,`ProcedureId`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `ProcedureId` (`ProcedureId`);

--
-- Indexes for table `nonconformites`
--
ALTER TABLE `nonconformites`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_reference_organisation` (`Reference`,`OrganisationId`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `DetecteParId` (`DetecteParId`),
  ADD KEY `ResponsableTraitementId` (`ResponsableTraitementId`),
  ADD KEY `ProcessusId` (`ProcessusId`);

--
-- Indexes for table `organisations`
--
ALTER TABLE `organisations`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Code` (`Code`);

--
-- Indexes for table `pointscontrole`
--
ALTER TABLE `pointscontrole`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `ResponsableId` (`ResponsableId`),
  ADD KEY `ProcessusId` (`ProcessusId`);

--
-- Indexes for table `procedures`
--
ALTER TABLE `procedures`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_code_organisation` (`Code`,`OrganisationId`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `ProcessusId` (`ProcessusId`),
  ADD KEY `ResponsableId` (`ResponsableId`);

--
-- Indexes for table `processus`
--
ALTER TABLE `processus`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_code_organisation` (`Code`,`OrganisationId`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `PiloteId` (`PiloteId`);

--
-- Indexes for table `processusacteurs`
--
ALTER TABLE `processusacteurs`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `ProcessusId` (`ProcessusId`),
  ADD KEY `UtilisateurId` (`UtilisateurId`);

--
-- Indexes for table `refreshtokens`
--
ALTER TABLE `refreshtokens`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Token` (`Token`),
  ADD KEY `idx_token` (`Token`(255)),
  ADD KEY `idx_user` (`UserId`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Username` (`Username`),
  ADD UNIQUE KEY `Email` (`Email`),
  ADD KEY `idx_username` (`Username`),
  ADD KEY `idx_email` (`Email`),
  ADD KEY `idx_role` (`Role`);

--
-- Indexes for table `versionsdocuments`
--
ALTER TABLE `versionsdocuments`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `DocumentId` (`DocumentId`),
  ADD KEY `OrganisationId` (`OrganisationId`),
  ADD KEY `EtabliParId` (`EtabliParId`),
  ADD KEY `VerifieParId` (`VerifieParId`),
  ADD KEY `ValideParId` (`ValideParId`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `auditlogs`
--
ALTER TABLE `auditlogs`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=56;

--
-- AUTO_INCREMENT for table `refreshtokens`
--
ALTER TABLE `refreshtokens`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=101;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=653024;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `actionscorrectives`
--
ALTER TABLE `actionscorrectives`
  ADD CONSTRAINT `actionscorrectives_ibfk_1` FOREIGN KEY (`NonConformiteId`) REFERENCES `nonconformites` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `actionscorrectives_ibfk_2` FOREIGN KEY (`ResponsableId`) REFERENCES `users` (`Id`),
  ADD CONSTRAINT `actionscorrectives_ibfk_3` FOREIGN KEY (`preuveEnregistrementId`) REFERENCES `enregistrement` (`id`);

--
-- Constraints for table `analysescauses`
--
ALTER TABLE `analysescauses`
  ADD CONSTRAINT `analysescauses_ibfk_1` FOREIGN KEY (`NonConformiteId`) REFERENCES `nonconformites` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `analysescauses_ibfk_2` FOREIGN KEY (`AnalyseParId`) REFERENCES `users` (`Id`);

--
-- Constraints for table `auditlogs`
--
ALTER TABLE `auditlogs`
  ADD CONSTRAINT `auditlogs_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE SET NULL;

--
-- Constraints for table `documents`
--
ALTER TABLE `documents`
  ADD CONSTRAINT `documents_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `documents_ibfk_2` FOREIGN KEY (`ProcessusId`) REFERENCES `processus` (`Id`) ON DELETE SET NULL;

--
-- Constraints for table `enregistrement`
--
ALTER TABLE `enregistrement`
  ADD CONSTRAINT `enregistrement_ibfk_1` FOREIGN KEY (`organisationid`) REFERENCES `organisations` (`Id`),
  ADD CONSTRAINT `enregistrement_ibfk_2` FOREIGN KEY (`processusid`) REFERENCES `processus` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `enregistrement_ibfk_3` FOREIGN KEY (`creeparid`) REFERENCES `users` (`Id`);

--
-- Constraints for table `evaluationspointcontrole`
--
ALTER TABLE `evaluationspointcontrole`
  ADD CONSTRAINT `evaluationspointcontrole_ibfk_1` FOREIGN KEY (`PointControleId`) REFERENCES `pointscontrole` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `evaluationspointcontrole_ibfk_2` FOREIGN KEY (`EvalueParId`) REFERENCES `users` (`Id`) ON DELETE SET NULL;

--
-- Constraints for table `historiquenonconformites`
--
ALTER TABLE `historiquenonconformites`
  ADD CONSTRAINT `historiquenonconformites_ibfk_1` FOREIGN KEY (`NonConformiteId`) REFERENCES `nonconformites` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `historiquenonconformites_ibfk_2` FOREIGN KEY (`ChangeParId`) REFERENCES `users` (`Id`);

--
-- Constraints for table `indicateurs`
--
ALTER TABLE `indicateurs`
  ADD CONSTRAINT `indicateurs_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `indicateurs_ibfk_3` FOREIGN KEY (`ResponsableId`) REFERENCES `users` (`Id`),
  ADD CONSTRAINT `indicateurs_ibfk_4` FOREIGN KEY (`ProcessusId`) REFERENCES `processus` (`Id`) ON DELETE SET NULL;

--
-- Constraints for table `indicateurvaleurs`
--
ALTER TABLE `indicateurvaleurs`
  ADD CONSTRAINT `indicateurvaleurs_ibfk_1` FOREIGN KEY (`IndicateurId`) REFERENCES `indicateurs` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `indicateurvaleurs_ibfk_2` FOREIGN KEY (`SaisiParId`) REFERENCES `users` (`Id`);

--
-- Constraints for table `instructions`
--
ALTER TABLE `instructions`
  ADD CONSTRAINT `instructions_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `instructions_ibfk_2` FOREIGN KEY (`ProcedureId`) REFERENCES `procedures` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `nonconformites`
--
ALTER TABLE `nonconformites`
  ADD CONSTRAINT `nonconformites_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `nonconformites_ibfk_3` FOREIGN KEY (`DetecteParId`) REFERENCES `users` (`Id`),
  ADD CONSTRAINT `nonconformites_ibfk_4` FOREIGN KEY (`ResponsableTraitementId`) REFERENCES `users` (`Id`) ON DELETE SET NULL,
  ADD CONSTRAINT `nonconformites_ibfk_5` FOREIGN KEY (`ProcessusId`) REFERENCES `processus` (`Id`) ON DELETE SET NULL;

--
-- Constraints for table `pointscontrole`
--
ALTER TABLE `pointscontrole`
  ADD CONSTRAINT `pointscontrole_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `pointscontrole_ibfk_3` FOREIGN KEY (`ResponsableId`) REFERENCES `users` (`Id`) ON DELETE SET NULL,
  ADD CONSTRAINT `pointscontrole_ibfk_4` FOREIGN KEY (`ProcessusId`) REFERENCES `processus` (`Id`) ON DELETE SET NULL;

--
-- Constraints for table `procedures`
--
ALTER TABLE `procedures`
  ADD CONSTRAINT `procedures_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `procedures_ibfk_2` FOREIGN KEY (`ProcessusId`) REFERENCES `processus` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `procedures_ibfk_3` FOREIGN KEY (`ResponsableId`) REFERENCES `users` (`Id`);

--
-- Constraints for table `processus`
--
ALTER TABLE `processus`
  ADD CONSTRAINT `processus_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `processus_ibfk_2` FOREIGN KEY (`PiloteId`) REFERENCES `users` (`Id`);

--
-- Constraints for table `processusacteurs`
--
ALTER TABLE `processusacteurs`
  ADD CONSTRAINT `processusacteurs_ibfk_1` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `processusacteurs_ibfk_2` FOREIGN KEY (`ProcessusId`) REFERENCES `processus` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `processusacteurs_ibfk_3` FOREIGN KEY (`UtilisateurId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `refreshtokens`
--
ALTER TABLE `refreshtokens`
  ADD CONSTRAINT `refreshtokens_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `versionsdocuments`
--
ALTER TABLE `versionsdocuments`
  ADD CONSTRAINT `versionsdocuments_ibfk_1` FOREIGN KEY (`DocumentId`) REFERENCES `documents` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `versionsdocuments_ibfk_2` FOREIGN KEY (`OrganisationId`) REFERENCES `organisations` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `versionsdocuments_ibfk_3` FOREIGN KEY (`EtabliParId`) REFERENCES `users` (`Id`),
  ADD CONSTRAINT `versionsdocuments_ibfk_4` FOREIGN KEY (`VerifieParId`) REFERENCES `users` (`Id`) ON DELETE SET NULL,
  ADD CONSTRAINT `versionsdocuments_ibfk_5` FOREIGN KEY (`ValideParId`) REFERENCES `users` (`Id`) ON DELETE SET NULL;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

// ── Réponse API (GET) ────────────────────────────────────────────────────
export interface ProcessusDto {
  id: string;
  code: string;
  nom: string;
  name?: string;              // alias anglais (templates)
  description?: string;
  type: ProcessType | string;
  finalites: string[];
  perimetres: string[];
  fournisseurs: string[];
  clients: string[];
  donneesEntree: string[];
  donneesSortie: string[];
  objectifs: string[];
  objective?: string;         // alias anglais (templates)
  pilote?: ProcessusPiloteDto;
  pilot?: string;             // alias anglais (templates)
  statut: ProcessStatut | string;
  status?: string;            // alias anglais (templates)
  conformityScore?: number;
  proceduresCount: number;
  documentsCount: number;
  acteursCount: number;
  actors?: ProcessusActeurDto[];
  kpis?: string[];
  dateCreation: string;
  dateModification?: string;
}

export interface ProcessusPiloteDto {
  id: number;
  nomComplet: string;
  email: string;
  fonction: string;
}

export interface ProcessusActeurDto {
  id: string;
  utilisateurId: number;
  nomComplet: string;
  email: string;
  typeActeur: TypeActeur;
  dateAffectation: string;
}

// ── Corps des requêtes (POST / PUT) ──────────────────────────────────────
export interface CreateProcessusDto {
  code: string;
  nom: string;
  description?: string;
  type: ProcessType | string;
  finalites: string[];
  perimetres: string[];
  fournisseurs: string[];
  clients: string[];
  donneesEntree: string[];
  donneesSortie: string[];
  objectifs: string[];
  piloteId: number;
  statut: ProcessStatut | string;
  acteurs: CreateProcessusActeurDto[];
}

export interface UpdateProcessusDto {
  nom?: string;
  description?: string;
  statut?: ProcessStatut | string;
}

export interface CreateProcessusActeurDto {
  utilisateurId: number;
  typeActeur: TypeActeur;
}

// ── Enums ────────────────────────────────────────────────────────────────
export enum ProcessType {
  PILOTAGE     = 'PILOTAGE',
  REALISATION  = 'REALISATION',
  SUPPORT      = 'SUPPORT',
  MANAGEMENT   = 'MANAGEMENT',
  OPERATIONNEL = 'OPERATIONNEL',
  MESURE       = 'MESURE'
}

export enum ProcessStatut {
  ACTIF   = 'ACTIF',
  INACTIF = 'INACTIF'
}

export enum TypeActeur {
  PILOTE       = 'PILOTE',
  COPILOTE     = 'COPILOTE',
  CONTRIBUTEUR = 'CONTRIBUTEUR',
  OBSERVATEUR  = 'OBSERVATEUR'
}

// ── Alias pour rétrocompatibilité ────────────────────────────────────────
export type Process = ProcessusDto;

export enum ProcessStatus {
  CONFORME     = 'Conforme',
  A_SURVEILLER = 'À surveiller',
  NON_CONFORME = 'Non conforme'
}

export enum ReviewFrequency {
  MENSUELLE     = 'Mensuelle',
  TRIMESTRIELLE = 'Trimestrielle',
  SEMESTRIELLE  = 'Semestrielle',
  ANNUELLE      = 'Annuelle'
}

export interface ProcessActor {
  id?: number;
  name: string;
  role: string;
  email?: string;
}

export interface DocumentModel {
  id?: string;
  processusId?: string;
  code: string;
  titre: string;
  typeDocument: string;
  description?: string;
  actif?: boolean;
  dateCreation?: string;
}

export interface NonConformity {
  id?: string;
  processusId?: string;
  reference?: string;
  gravite?: string;
  statut?: string;
}
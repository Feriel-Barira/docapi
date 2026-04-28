// src/app/shared/models/non-conformite.model.ts

export type NCGravite = 'MINEURE' | 'MAJEURE' | 'CRITIQUE';
export type NCStatut = 'OUVERTE' | 'ANALYSE' | 'ACTION_EN_COURS' | 'CLOTUREE';
export type NCType = 'PRODUIT_SERVICE' | 'PROCESSUS' | 'SYSTEME' | 'ENVIRONNEMENT';
export type NCNature = 'REELLE' | 'POTENTIELLE';
export type NCSource = 'AUDIT' | 'RECLAMATION' | 'POINT_CONTROLE' | 'INSPECTION' | 'OBSERVATION';
export type ACType = 'CURATIVE' | 'CORRECTIVE' | 'PREVENTIVE';
export type ACStatut = 'PLANIFIEE' | 'EN_COURS' | 'REALISEE' | 'VERIFIEE';

export interface ActionCorrective {
  id: string;
  ncId: string;
  description: string;
  type: ACType;
  responsableId?: number;
  responsableNom?: string;
  echeance?: string;
  statut: ACStatut;
  dateCreation?: string;
   preuveEnregistrementId?: string;
}

export interface AnalyseCause {
  id: string;
  methodeAnalyse: string;
  description: string;
  dateAnalyse: string;
  analyseParId: number;
  analyseParNom?: string;
}

export interface HistoriqueNonConformite {
  id: string;
  ancienStatut: NCStatut;
  nouveauStatut: NCStatut;
  dateChangement: string;
  changeParId: number;
  changeParNom?: string;
  commentaire?: string;
}

export interface NonConformite {
  id: string;
  reference: string;
  description: string;
  type: NCType;
  nature: NCNature;
  source: NCSource;
  gravite: NCGravite;
  statut: NCStatut;
  processusId?: string;
  processusCode?: string;
  responsableId?: number;
  responsableNom?: string;
  detecteParId?: number;
  detecteParNom?: string;
  dateDetection: string;
  dateCreation: string;
  actionsCorrectives?: ActionCorrective[];
  avancementAC?: number;
  analyseCause?: AnalyseCause;
  historique?: HistoriqueNonConformite[];
}

export interface CreateNonConformiteDto {
  description: string;
  type: NCType;
  nature: NCNature;
  source: NCSource;
  gravite: NCGravite;
  processusId?: string;
  responsableTraitementId?: number;  
  detecteParId?: number;
  dateDetection: string;
  actionInitiale?: {
    description: string;
    type: ACType;
    responsableId?: number;
    echeance?: string;
  };
}

export interface CreateACDto {
  description: string;
  type: ACType;
  responsableId?: number;
  echeance?: string;
}
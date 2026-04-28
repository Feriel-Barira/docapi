// src/app/shared/models/audit.model.ts

export type AuditType = 'DOCUMENTAIRE' | 'OPERATIONNEL' | 'REGLEMENTAIRE';
export type AuditFrequence = 'QUOTIDIEN' | 'HEBDOMADAIRE' | 'MENSUEL' | 'TRIMESTRIEL' | 'ANNUEL';

export interface Evaluation {
  id: string;
  pointControleId: string;
  dateEvaluation: string;
  conforme: boolean;
  commentaire?: string;
  evalueParId?: number;
  evalueParNom?: string;
  dateCreation?: string;
}

export interface Audit {
  id: string;
  organisationId?: string;
  processusId?: string;
  processusCode?: string;
  processusNom?: string;  // ← AJOUTÉ
  nom: string;
  description?: string;
  type: AuditType;
  frequence: AuditFrequence;
  responsableId?: number;
  responsableNom?: string;
  actif: boolean;
  evaluations?: Evaluation[];
  derniereEvaluation?: Evaluation;
  dateCreation?: string;
  dateModification?: string;
}

export interface CreateAuditDto {
  nom: string;
  description?: string;
  processusId?: string;
  type: AuditType;
  frequence: AuditFrequence;
  responsableId?: number;
  actif: boolean;
}

export interface CreateEvaluationDto {
  dateEvaluation: string;
  conforme: boolean;
  commentaire?: string;
  evalueParId?: number;
}
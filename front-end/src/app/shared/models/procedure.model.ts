export interface Procedure {
  id: string;
  organisationId: string;
  processusId: string;
  code: string;
  titre: string;
  objectif: string;
  domaineApplication: string;
  description: string;
  responsableId: string;
  statut: 'ACTIF' | 'INACTIF';
  dateCreation: string;
  processusCode?: string;  // ← AJOUTER
  processusNom?:  string; 
  // Relations
  processus?: {
    id: string;
    code: string;
    nom: string;
  };
  responsable?: {
    id: string;
    nom: string;
    prenom: string;
    fonction: string;
  };
  instructions?: Instruction[];
  documentsAssocies?: DocumentLie[];
}

export interface Instruction {
  id: string;
  organisationId: string;
  procedureId: string;
  code: string;
  titre: string;
  description: string;
  statut: 'ACTIF' | 'INACTIF';
  dateCreation: string;
}

export interface DocumentLie {
  id: string;
  code: string;
  titre: string;
  typeDocument: 'REFERENCE' | 'TRAVAIL';
  version?: string;
}

export interface CreateProcedureDto {
  code: string;
  titre: string;
  objectif: string;
  domaineApplication: string;
  description: string;
  processusId: string;
  responsableId: string;
  statut: 'ACTIF' | 'INACTIF';
  instructions?: Omit<Instruction, 'id' | 'dateCreation' | 'procedureId' | 'organisationId'>[];
  documentsAssocies?: string[];
}
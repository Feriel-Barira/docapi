// src/app/shared/models/document.model.ts

export type DocTypeDocument = 'REFERENCE' | 'TRAVAIL';
export type DocStatut = 'BROUILLON' | 'EN_REVISION' | 'VALIDE' | 'OBSOLETE';

export interface VersionDocument {
  id:                  string;
  documentId:          string;
  numeroVersion:       string;
  statut:              DocStatut;
  commentaireRevision: string;
  fichierPath?:        string;
  etabliParId?:        string;
  etabliParNom?:       string;
  dateEtablissement?:  string;
  verifieParId?:       string;
  verifieParNom?:      string;
  dateVerification?:   string;
  valideParId?:        string;
  valideParNom?:       string;
  dateValidation?:     string;
  dateMiseEnVigueur?:  string;
}

export interface DocumentISO {
  id:                     string;
  organisationId?:        string;
  code:                   string;
  titre:                  string;
  typeDocument:           DocTypeDocument;
  processusId?:           string;
  processusCode?:         string;
  description?:           string;
  actif:                  boolean;
  dateCreation:           string;
  // Version courante (calculée)
  derniereVersion?:       string;
  derniereVersionStatut?: DocStatut;
  dateMiseAJour?:         string;
  // Responsable
  responsableId?:         string;
  responsableNom?:        string;
  responsableInitiales?:  string;
  responsableCouleur?:    string;
  // Relations
  versions?:              VersionDocument[];
  proceduresAssocieesIds?: string[];
}

export interface CreateDocumentDto {
  code:          string;
  titre:         string;
  typeDocument:  DocTypeDocument;
  processusId?:  string;
  description?:  string;
  actif:         boolean;
  proceduresAssocieesIds?: string[];
  versionInitiale?: CreateVersionDto; 
}

export interface CreateVersionDto {
  numeroVersion:        string;
  statut:               DocStatut;
  commentaireRevision?: string;
  fichierPath?:         string;
  etabliParId?:         string;
  verifieParId?:        string;
  valideParId?:         string;
  dateEtablissement?:   string;
  dateVerification?:    string;
  dateValidation?:      string;
  dateMiseEnVigueur?:   string;
}
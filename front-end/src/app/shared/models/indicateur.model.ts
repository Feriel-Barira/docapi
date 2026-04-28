export type IndFrequence = 'QUOTIDIEN' | 'HEBDOMADAIRE' | 'MENSUEL' | 'TRIMESTRIEL' | 'ANNUEL';
export type IndStatut   = 'ACTIF' | 'INACTIF';

export interface ValeurIndicateur {
  id: string;
  indicateurId: string;
  periode: string;
  valeur: number;
  dateMesure: string;
  commentaire?: string;
  saisiParNom?: string;
}

export interface Indicateur {
  id: string;
  code: string;
  nom: string;
  description?: string;
  processusId?: string;
  processusNom?: string; 
  processusCode?: string;
  responsableId?: number;
  responsableNom?: string;
  unite?: string;
  methodeCalcul: string;
  valeurCible?: number;
  seuilAlerte?: number;
  frequence: IndFrequence;
  statut: IndStatut;
  dateCreation: string;
  derniereValeur?: number;
  dernierePeriode?: string;
  valeurs?: ValeurIndicateur[];
}

export interface CreateIndicateurDto {
  code: string;
  nom: string;
  description?: string;
  processusId?: string;
  responsableId?: number;
  unite?: string;
  methodeCalcul: string;
  valeurCible?: number;
  seuilAlerte?: number;
   frequenceMesure: IndFrequence;
  actif: boolean;
}

export interface CreateValeurDto {
  periode: string;
  valeur: number;
  dateMesure: string;
  commentaire?: string;
  saisiParId: number;
}
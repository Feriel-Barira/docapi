export interface EnregistrementDto {
  id: string;
  processusId: string;
  processusNom?: string;
  actionCorrectiveId?: string;
  typeEnregistrement: string;
  reference: string;
  description: string;
  fichierPath: string;
  dateEnregistrement: string;
  creeParId: number;
  creeParNom?: string;
  processusCode?: string;
}
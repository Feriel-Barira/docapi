import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardDto {
  statistiques: StatistiquesGeneralesDto;
  alertes: AlerteDto[];
  activitesRecentes: ActiviteRecenteDto[];
  repartitionNonConformites: RepartitionNcDto;
  indicateursHorsCible: IndicateurResumDto[];
}

export interface StatistiquesGeneralesDto {
  totalProcessus: number;
  processusActifs: number;
  totalProcedures: number;
  proceduresActives: number;
  totalDocuments: number;
  documentsValides: number;
  documentsEnAttente: number;
  totalNonConformites: number;
  ncOuvertes: number;
  ncAnalyse: number;
  ncActionEnCours: number;
  ncCloturees: number;
  totalActionsCorrectives: number;
  actionsEnRetard: number;
  actionsEcheanceProche: number;
  totalIndicateurs: number;
  indicateursActifs: number;
}

export interface AlerteDto {
  type: string;
  niveau: string;   // DANGER, WARNING, INFO
  message: string;
  entityId?: string;
  entityType?: string;
  date: string;
}

export interface ActiviteRecenteDto {
  action: string;
  entityType: string;
  entityNom: string;
  username: string;
  date: string;
}

export interface RepartitionNcDto {
  mineure: number;
  majeure: number;
  critique: number;
  parAudit: number;
  parPointControle: number;
  parReclamation: number;
  parAutre: number;
}

export interface IndicateurResumDto {
  id: string;
  nom: string;
  code: string;
  valeurCible?: number;
  derniereValeur?: number;
  periode?: string;
  unite: string;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {

  private readonly API = `${environment.apiUrl}/Dashboard`;

  constructor(private http: HttpClient) {}

  getDashboard(organisationId: string): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(this.API, {
      params: new HttpParams().set('organisationId', organisationId)
    });
  }
}
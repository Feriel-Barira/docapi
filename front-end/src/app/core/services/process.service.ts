import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ProcessusDto } from '../../shared/models/process.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ProcessService {

  private readonly API = `${environment.apiUrl}/Processus`;

  constructor(private http: HttpClient, private authService: AuthService ) {}

  // ── GET ALL ─────────────────────
getProcesses(): Observable<ProcessusDto[]> {
  const orgId = localStorage.getItem('auth_organisation_id') ?? '';
  let params = new HttpParams();
  if (orgId) params = params.set('organisationId', orgId);

  return this.http.get<any[]>(this.API, { params }).pipe(
    map(list => {
      console.log('Premier processus RAW:', JSON.stringify(list[0]));
      return list.map(p => this.mapOne(p));
    })
  );
}

  // ── CREATE ─────────────────────
  createProcess(dto: any): Observable<ProcessusDto> {
  const orgId = localStorage.getItem('auth_organisation_id') ?? '';
  return this.http.post<any>(`${this.API}?organisationId=${orgId}`, dto).pipe(
    map(p => this.mapOne(p))
  );
}

  // ── UPDATE ─────────────────────
  updateProcess(id: string, dto: any): Observable<ProcessusDto> {
    return this.http.put<any>(`${this.API}/${id}`, dto).pipe(
      map(p => this.mapOne(p))
    );
  }

  // ── DELETE ─────────────────────
  deleteProcess(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API}/${id}`);
  }

  // ── MAPPING ─────────────────────
  private mapOne(p: any): ProcessusDto {
  console.log('📦 Mapping processus:', p);  // ← Debug pour voir ce que backend envoie
  
  return {
    id: p.id || p.Id,
    code: p.code || p.Code,           // ← Supporte Code et code
    nom: p.nom || p.Nom,              // ← Supporte Nom et nom
    name: p.nom || p.Nom,             // ← Pour le template
    description: p.description || p.Description,
    objective: p.description || p.Description,
    type: p.type || p.Type,
    statut: p.statut || p.Statut,
    status: p.statut || p.Statut,

    finalites: this.parseJSON(p.finalites || p.Finalites),
    perimetres: this.parseJSON(p.perimetres || p.Perimetres),
    objectifs: this.parseJSON(p.objectifs || p.Objectifs),
    fournisseurs: this.parseJSON(p.fournisseurs || p.Fournisseurs),
    clients: this.parseJSON(p.clients || p.Clients),
    donneesEntree: this.parseJSON(p.donneesEntree || p.DonneesEntree),
    donneesSortie: this.parseJSON(p.donneesSortie || p.DonneesSortie),

    pilot: p.PiloteNom || p.piloteNom || '—',
    pilote: p.pilote ? p.pilote : { 
      id: p.PiloteId || p.piloteId,
      nomComplet: p.PiloteNom || p.piloteNom,
      email: p.PiloteEmail,
      fonction: ''
    },
    
    proceduresCount: p.proceduresCount || p.ProceduresCount || 0,
    documentsCount: p.documentsCount || p.DocumentsCount || 0,
    acteursCount: p.acteursCount || p.ActeursCount || 0,
    actors: [],
    conformityScore: 80,
    dateCreation: p.dateCreation || p.DateCreation
  };
}
checkProcessusCode(code: string, excludeId?: string): Observable<boolean> {
  const organisationId = this.authService.getOrganisationId(); // à adapter selon ton AuthService
  let params = new HttpParams()
    .set('code', code)
    .set('organisationId', organisationId);
  if (excludeId) {
    params = params.set('excludeId', excludeId);
  }
  return this.http.get<{ exists: boolean }>(`${environment.apiUrl}/Processus/check-code`, { params })
    .pipe(map(res => res.exists));
}

// Ajouter cette méthode helper pour parser les JSON
private parseJSON(value: any): string[] {
  if (!value) return [];
  if (Array.isArray(value)) return value;
  if (typeof value === 'string') {
    try {
      const parsed = JSON.parse(value);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }
  return [];
}
}
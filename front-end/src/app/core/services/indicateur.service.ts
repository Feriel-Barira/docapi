import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  Indicateur,
  ValeurIndicateur,
  CreateIndicateurDto,
  CreateValeurDto,
  IndFrequence,
} from '../../shared/models/indicateur.model';

@Injectable({ providedIn: 'root' })
export class IndicateurService {

  private readonly API = `${environment.apiUrl}/Indicateur`;

  constructor(private http: HttpClient) {}

  private getOrgId(): string {
    return localStorage.getItem('auth_organisation_id') || '';
  }

  private getHeaders() {
    let token = localStorage.getItem('auth_token');
  if (!token) token = localStorage.getItem('token');
  if (!token) token = localStorage.getItem('access_token');
  
  console.log('🔑 Token utilisé:', token ? 'Présent' : 'MANQUANT');
    return { Authorization: `Bearer ${token}` };
  }

  // ==================== DROPDOWNS ====================
  
  getProcessusList(): Observable<{ id: string; code: string; nom: string }[]> {
    const orgId = this.getOrgId();
    const params = new HttpParams().set('organisationId', orgId);
    return this.http.get<any[]>(`${this.API}/processus-list`, { params, headers: this.getHeaders() }).pipe(
      map(list => list.map(p => ({
        id: p.Id || p.id,
        code: p.Code || p.code,
        nom: p.Nom || p.nom
      })))
    );
  }

  getResponsablesList(): Observable<{ id: number; nom: string }[]> {
    const orgId = this.getOrgId();
    const params = new HttpParams().set('organisationId', orgId);
    return this.http.get<any[]>(`${this.API}/responsables`, { params, headers: this.getHeaders() }).pipe(
      map(list => list.map(r => ({
        id: r.Id || r.id,
        nom: r.Nom || r.nom
      })))
    );
  }

  // ==================== INDICATEURS ====================

  getIndicateurs(): Observable<Indicateur[]> {
  const orgId = this.getOrgId();
  const params = new HttpParams().set('organisationId', orgId);
  return this.http.get<any[]>(this.API, { params, headers: this.getHeaders() }).pipe(
    tap(data => console.log('API Response - Actif values:', data.map(i => ({ id: i.id, Actif: i.Actif })))),
    map(list => list.map(i => this.mapIndicateur(i)))
  );
}

  getIndicateurById(id: string): Observable<Indicateur> {
    return this.http.get<any>(`${this.API}/${id}`, { headers: this.getHeaders() }).pipe(
      map(i => this.mapIndicateur(i))
    );
  }

  createIndicateur(dto: CreateIndicateurDto): Observable<Indicateur> {
    const orgId = this.getOrgId();
    const params = new HttpParams().set('organisationId', orgId);
    return this.http.post<any>(this.API, dto, { params, headers: this.getHeaders() }).pipe(
      map(i => this.mapIndicateur(i))
    );
  }
updateIndicateur(id: string, dto: CreateIndicateurDto): Observable<Indicateur> {
  const updatePayload: any = {};
  
  if (dto.code !== undefined) updatePayload.code = dto.code;
  if (dto.nom !== undefined) updatePayload.nom = dto.nom;
  if (dto.description !== undefined) updatePayload.description = dto.description;
  if (dto.methodeCalcul !== undefined) updatePayload.methodeCalcul = dto.methodeCalcul;
  if (dto.unite !== undefined) updatePayload.unite = dto.unite;
  if (dto.valeurCible !== undefined) updatePayload.valeurCible = dto.valeurCible;
  if (dto.seuilAlerte !== undefined) updatePayload.seuilAlerte = dto.seuilAlerte;
  // ✅ CORRECTION ICI
  if (dto.frequenceMesure !== undefined) updatePayload.frequenceMesure = dto.frequenceMesure.toUpperCase();
  if (dto.responsableId !== undefined) updatePayload.responsableId = dto.responsableId;
  if (dto.actif !== undefined) updatePayload.actif = dto.actif;
  if (dto.processusId !== undefined) updatePayload.processusId = dto.processusId;
  
  console.log('Update payload:', updatePayload);
  
  return this.http.put<any>(`${this.API}/${id}`, updatePayload, { headers: this.getHeaders() }).pipe(
    map(i => this.mapIndicateur(i))
  );
}

  deleteIndicateur(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API}/${id}`, { headers: this.getHeaders() });
  }

  // ==================== VALEURS ====================

  getValeurs(indicateurId: string, limit: number = 12): Observable<ValeurIndicateur[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<any[]>(`${this.API}/${indicateurId}/valeurs`, { params, headers: this.getHeaders() }).pipe(
      map(list => list.map(v => this.mapValeur(v, indicateurId)))
    );
  }

  addValeur(indicateurId: string, dto: CreateValeurDto): Observable<ValeurIndicateur> {
    return this.http.post<any>(`${this.API}/${indicateurId}/valeurs`, dto, { headers: this.getHeaders() }).pipe(
      map(v => this.mapValeur(v, indicateurId))
    );
  }

  updateValeur(valeurId: string, dto: CreateValeurDto): Observable<ValeurIndicateur> {
    return this.http.put<any>(`${this.API}/valeurs/${valeurId}`, dto, { headers: this.getHeaders() }).pipe(
      map(v => this.mapValeur(v, ''))
    );
  }

  deleteValeur(valeurId: string): Observable<void> {
    return this.http.delete<void>(`${this.API}/valeurs/${valeurId}`, { headers: this.getHeaders() });
  }

  // ==================== MAPPERS ====================
private mapIndicateur(r: any): Indicateur {
  const valeurs: ValeurIndicateur[] = (r.Valeurs || r.valeurs || [])
    .map((v: any) => this.mapValeur(v, r.Id || r.id));

  // Récupérer les objets imbriqués
  const processusObj = r.processus || r.Processus;
  const responsableObj = r.responsable || r.Responsable;

  // 🔧 Lire la bonne propriété (Actif avec majuscule)
  const actifValue = r.Actif !== undefined ? r.Actif : r.actif;
  console.log(`MapIndicateur - ${r.Code}: Actif reçu = ${actifValue}`);

  return {
    id: r.Id || r.id,
    code: r.Code || r.code,
    nom: r.Nom || r.nom,
    description: r.Description || r.description,
    methodeCalcul: r.MethodeCalcul || r.methodeCalcul,
    unite: r.Unite || r.unite,
    valeurCible: r.ValeurCible ?? r.valeurCible,
    seuilAlerte: r.SeuilAlerte ?? r.seuilAlerte,
    frequence: (r.FrequenceMesure || r.frequenceMesure || 'MENSUEL') as IndFrequence,
    // 🔧 CORRECTION : 1 = ACTIF, 0 = INACTIF
    statut: (actifValue === 1 || actifValue === true || actifValue === 'true') ? 'ACTIF' : 'INACTIF',
    dateCreation: (r.DateCreation || r.dateCreation || '').split('T')[0],
    derniereValeur: r.DerniereValeur ?? r.derniereValeur,
    dernierePeriode: r.DernierePeriode || r.dernierePeriode,
    
    // Processus
    processusId: processusObj?.id || r.ProcessusId || r.processusId,
    processusCode: processusObj?.code,
    processusNom: processusObj?.nom,
    
    // Responsable
    responsableId: responsableObj?.id || r.ResponsableId || r.responsableId,
    responsableNom: responsableObj?.nomComplet,
    
    valeurs
  };
}
  private mapValeur(v: any, indicateurId: string): ValeurIndicateur {
    return {
      id: v.Id || v.id,
      indicateurId: indicateurId || v.IndicateurId || v.indicateurId,
      periode: v.Periode || v.periode,
      valeur: v.Valeur ?? v.valeur ?? 0,
      dateMesure: (v.DateMesure || v.dateMesure || '').split('T')[0],
      commentaire: v.Commentaire || v.commentaire,
      saisiParNom: v.SaisiPar?.NomComplet || v.SaisiPar?.nomComplet || v.saisiParNom
    };
  }
updateStatut(id: string, actif: boolean): Observable<Indicateur> {
  console.log('=== updateStatut ===');
  console.log('ID:', id);
  console.log('Nouveau statut (actif):', actif);
  
  // 🔧 Forcer la conversion en boolean
  return this.http.patch<any>(`${this.API}/${id}/statut`, { Actif: !!actif }, { 
    headers: this.getHeaders()
  }).pipe(
    map(i => this.mapIndicateur(i))
  );
}
}
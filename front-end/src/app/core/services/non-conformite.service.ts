// src/app/core/services/non-conformite.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, of, catchError, throwError } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  NonConformite, ActionCorrective, AnalyseCause, HistoriqueNonConformite,
  CreateNonConformiteDto, CreateACDto,
  NCGravite, NCStatut, NCType, NCNature, NCSource, ACType, ACStatut
} from '../../shared/models/non-conformite.model';

@Injectable({ providedIn: 'root' })
export class NonConformiteService {

  private readonly API    = `${environment.apiUrl}/NonConformite`;
  private readonly API_AC = `${environment.apiUrl}/ActionCorrective`;

  constructor(private http: HttpClient) {}

  private getOrgId(): string {
    return localStorage.getItem('auth_organisation_id') || '';
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  // ── GET ALL ──────────────────────────────────────────────
  getNonConformites(): Observable<NonConformite[]> {
    const orgId = this.getOrgId();
    const params = new HttpParams().set('organisationId', orgId);
    return this.http.get<any[]>(this.API, { params, headers: this.getHeaders() }).pipe(
      map(data => data.map(item => this.mapOne(item))),
      catchError(error => {
        console.error('Erreur chargement NC:', error);
        return of([]);
      })
    );
  }

  // ── GET BY ID ────────────────────────────────────────────
  getNonConformiteById(id: string): Observable<NonConformite> {
    return this.http.get<any>(`${this.API}/${id}`, { headers: this.getHeaders() }).pipe(
      map(item => this.mapOne(item)),
      catchError(error => {
        console.error('Erreur chargement NC par ID:', error);
        return throwError(() => new Error('NC non trouvée'));
      })
    );
  }

  // ── CREATE ───────────────────────────────────────────────
  createNonConformite(dto: CreateNonConformiteDto): Observable<NonConformite> {
    console.log('DTO complet:', dto);
    const orgId = this.getOrgId();
    const params = new HttpParams().set('organisationId', orgId);
    return this.http.post<any>(this.API, dto, { params, headers: this.getHeaders() }).pipe(
      map(item => this.mapOne(item)),
      catchError(error => {
        console.error('Erreur création NC:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur création'));
      })
    );
  }

  // ── UPDATE ───────────────────────────────────────────────
  updateNonConformite(id: string, dto: Partial<NonConformite>): Observable<NonConformite> {
    return this.http.put<any>(`${this.API}/${id}`, dto, { headers: this.getHeaders() }).pipe(
      map(item => this.mapOne(item)),
      catchError(error => {
        console.error('Erreur mise à jour NC:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur mise à jour'));
      })
    );
  }

  // ── UPDATE STATUT ────────────────────────────────────────
  updateStatut(id: string, statut: NCStatut, commentaire?: string): Observable<NonConformite> {
    return this.http.put<any>(
      `${this.API}/${id}/statut?statut=${statut}`,
      commentaire || null,
      { headers: this.getHeaders() }
    ).pipe(
      map(item => this.mapOne(item)),
      catchError(error => {
        console.error('Erreur changement statut:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur changement statut'));
      })
    );
  }

  // ── DELETE ───────────────────────────────────────────────
  deleteNonConformite(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API}/${id}`, { headers: this.getHeaders() }).pipe(
      catchError(error => {
        console.error('Erreur suppression NC:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur suppression'));
      })
    );
  }

  // ── ADD AC ───────────────────────────────────────────────
  addActionCorrective(ncId: string, dto: CreateACDto): Observable<ActionCorrective> {
    return this.http.post<any>(`${this.API_AC}/nonconformite/${ncId}`, dto, { headers: this.getHeaders() }).pipe(
      map(item => this.mapAC(item, ncId)),
      catchError(error => {
        console.error('Erreur ajout AC:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur ajout action corrective'));
      })
    );
  }

 // ── UPDATE AC STATUT ─────────────────────────────────────
updateACStatut(ncId: string, acId: string, statut: string): Observable<void> {
    return this.http.put<void>(`${this.API_AC}/${acId}`, { statut }, { headers: this.getHeaders() }).pipe(
        catchError(error => {
            console.error('Erreur mise à jour statut AC:', error);
            return throwError(() => new Error(error.error?.message || 'Erreur mise à jour'));
        })
    );
}

  // ── DELETE AC ────────────────────────────────────────────
  deleteAC(ncId: string, acId: string): Observable<void> {
    return this.http.delete<void>(`${this.API_AC}/${acId}`, { headers: this.getHeaders() }).pipe(
      catchError(error => {
        console.error('Erreur suppression AC:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur suppression'));
      })
    );
  }

  // ── GET AC BY NC ─────────────────────────────────────────
  getActionsByNC(ncId: string): Observable<ActionCorrective[]> {
    return this.http.get<any[]>(`${this.API_AC}/nonconformite/${ncId}`, { headers: this.getHeaders() }).pipe(
      map(data => data.map(a => this.mapAC(a, ncId))),
      catchError(error => {
        console.error('Erreur chargement AC:', error);
        return of([]);
      })
    );
  }

  // ── ANALYSE DES CAUSES ───────────────────────────────────
  addAnalyse(ncId: string, dto: { methodeAnalyse: string; description: string }): Observable<AnalyseCause> {
    return this.http.post<any>(`${this.API}/${ncId}/analyse`, dto, { headers: this.getHeaders() }).pipe(
      map(item => this.mapAnalyse(item)),
      catchError(error => {
        console.error('Erreur ajout analyse:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur ajout analyse'));
      })
    );
  }

  updateAnalyse(ncId: string, analyseId: string, dto: { methodeAnalyse: string; description: string }): Observable<AnalyseCause> {
    return this.http.put<any>(`${this.API}/analyse/${analyseId}`, dto, { headers: this.getHeaders() }).pipe(
      map(item => this.mapAnalyse(item)),
      catchError(error => {
        console.error('Erreur mise à jour analyse:', error);
        return throwError(() => new Error(error.error?.message || 'Erreur mise à jour analyse'));
      })
    );
  }

  // ── HISTORIQUE ───────────────────────────────────────────
 // ── HISTORIQUE ───────────────────────────────────────────
getHistorique(ncId: string): Observable<HistoriqueNonConformite[]> {
  return this.http.get<any[]>(`${this.API}/${ncId}/historique`, { headers: this.getHeaders() }).pipe(
    map(list => list.map(h => ({
      id: h.Id || h.id,
      ancienStatut: h.AncienStatut || h.ancienStatut,
      nouveauStatut: h.NouveauStatut || h.nouveauStatut,
      // ✅ CORRECTION : Garder la date ET l'heure, ne pas split
      dateChangement: h.DateChangement || h.dateChangement,
      changeParId: h.ChangeParId || h.changeParId,
      changeParNom: h.ChangePar?.NomComplet || h.changePar?.nomComplet,
      commentaire: h.Commentaire || h.commentaire
    }))),
    catchError(error => {
      console.error('Erreur chargement historique:', error);
      return of([]);
    })
  );
}

 getResponsablesFromApi(): Observable<{ id: number; nom: string }[]> {
  const orgId = this.getOrgId();
  const params = new HttpParams().set('organisationId', orgId);
  // ✅ L'URL doit être /Users/responsables
  const url = `${environment.apiUrl}/Users/responsables`;
  console.log('URL appelée:', url);
  
  return this.http.get<any[]>(url, { params, headers: this.getHeaders() }).pipe(
    tap(data => console.log('Réponse:', data)),
    map(list => list.map(u => ({
      id: u.Id || u.id,
      nom: u.Nom || u.nom
    }))),
    catchError(error => {
      console.error('Erreur:', error);
      return of([]);
    })
  );
}
  getProcessusFromApi(): Observable<{ id: string; code: string; nom: string }[]> {
    const orgId = this.getOrgId();
    const params = new HttpParams().set('organisationId', orgId);
    return this.http.get<any[]>(`${environment.apiUrl}/Processus`, { params, headers: this.getHeaders() }).pipe(
      map(list => list.map(p => ({
        id: p.Id || p.id,
        code: p.Code || p.code,
        nom: p.Nom || p.nom
      }))),
      catchError(error => {
        console.error('Erreur chargement processus:', error);
        return of([]);
      })
    );
  }

  // ── MAPPERS ──────────────────────────────────────────────
  private mapOne(r: any): NonConformite {
    const acs: ActionCorrective[] = (r.ActionsCorrectives || r.actionsCorrectives || [])
      .map((a: any) => this.mapAC(a, r.Id || r.id));
    const total = acs.length;
    const done = acs.filter(a => a.statut === 'REALISEE' || a.statut === 'VERIFIEE').length;

    const processus = r.Processus || r.processus;
    const detectePar = r.DetectePar || r.detectePar;
    const responsable = r.ResponsableTraitement || r.responsableTraitement;

    return {
      id: r.Id || r.id,
      reference: r.Reference || r.reference,
      description: r.Description || r.description,
      type: (r.Type || r.type || 'PRODUIT_SERVICE') as NCType,
      nature: (r.Nature || r.nature || 'REELLE') as NCNature,
      source: (r.Source || r.source) as NCSource,
      gravite: (r.Gravite || r.gravite) as NCGravite,
      statut: (r.Statut || r.statut) as NCStatut,
      processusId: processus?.Id || processus?.id || r.ProcessusId || r.processusId,
      processusCode: processus?.Code || processus?.code,
      responsableId: responsable?.Id || responsable?.id,
      responsableNom: responsable?.NomComplet || responsable?.nomComplet,
      detecteParId: detectePar?.Id || detectePar?.id,
      detecteParNom: detectePar?.NomComplet || detectePar?.nomComplet,
      dateDetection: (r.DateDetection || r.dateDetection || '').split('T')[0],
      dateCreation: (r.DateCreation || r.dateCreation || '').split('T')[0],
      actionsCorrectives: acs,
      avancementAC: total > 0 ? Math.round((done / total) * 100) : 0,
      analyseCause: (r.AnalyseCause || r.analyseCause) ? this.mapAnalyse(r.AnalyseCause || r.analyseCause) : undefined
    };
  }

 private mapAC(a: any, ncId: string): ActionCorrective {
    // 🔧 Lire le responsable (peut être 'responsable' ou 'Responsable')
    const responsableObj = a.responsable || a.Responsable;
    
    console.log('Responsable trouvé:', responsableObj);
    
    return {
        id: a.Id || a.id,
        ncId,
        description: a.Description || a.description,
        type: (a.Type || a.type) as ACType,
        responsableId: responsableObj?.Id || responsableObj?.id || a.ResponsableId || a.responsableId,
        responsableNom: responsableObj?.nomComplet || responsableObj?.NomComplet,
        echeance: (a.DateEcheance || a.dateEcheance || '').split('T')[0],
        statut: (a.Statut || a.statut) as ACStatut,
        dateCreation: (a.DateCreation || a.dateCreation || '').split('T')[0],
         preuveEnregistrementId: a.PreuveEnregistrementId || a.preuveEnregistrementId   
    };
}
  private mapAnalyse(item: any): AnalyseCause {
    return {
      id: item.Id || item.id,
      methodeAnalyse: item.MethodeAnalyse || item.methodeAnalyse,
      description: item.Description || item.description,
      dateAnalyse: (item.DateAnalyse || item.dateAnalyse || '').split('T')[0],
      analyseParId: item.AnalyseParId || item.analyseParId,
      analyseParNom: item.AnalysePar?.NomComplet || item.analysePar?.nomComplet
    };
  }
  // ── ATTACHER / DETACHER PREUVE ────────────────────────────
attacherPreuve(acId: string, enregId: string): Observable<void> {
  const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
  return this.http.put<void>(`${this.API_AC}/${acId}/preuve`, JSON.stringify(enregId), { headers });
}

detacherPreuve(acId: string): Observable<void> {
  return this.http.delete<void>(`${this.API_AC}/${acId}/preuve`);
}
  
}
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Audit, Evaluation, CreateAuditDto, CreateEvaluationDto, AuditType, AuditFrequence } from '../../shared/models/audit.model';

@Injectable({ providedIn: 'root' })
export class AuditService {

  private readonly baseUrl = `${environment.apiUrl}/PointControle`;
  private readonly processusUrl = `${environment.apiUrl}/Processus`;

  constructor(private http: HttpClient) {}

  // ===========================
  // 🔐 AUTH HEADERS
  // ===========================
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  // ===========================
  // 📌 ORG ID
  // ===========================
  private getOrgId(): string {
    const orgId = localStorage.getItem('auth_organisation_id') || '';
    console.log('Organisation ID:', orgId);
    return orgId;
  }

  // ===========================
  // 🔵 AUDITS
  // ===========================
  getAudits(): Observable<Audit[]> {
    const params = new HttpParams().set('organisationId', this.getOrgId());

    return this.http.get<any[]>(this.baseUrl, {
      params,
      headers: this.getHeaders()
    }).pipe(
      tap(data => console.log('API audits:', data)),
      map(list => list.map(item => this.mapAudit(item)))
    );
  }

  getAuditById(id: string): Observable<Audit> {
    return this.http.get<any>(`${this.baseUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      map(item => this.mapAudit(item))
    );
  }

  createAudit(dto: CreateAuditDto): Observable<Audit> {
    const params = new HttpParams().set('organisationId', this.getOrgId());
    
    console.log('=== createAudit ===');
    console.log('URL:', this.baseUrl);
    console.log('Params:', params.toString());
    console.log('DTO reçu:', dto);
    console.log('processusId dans DTO:', dto.processusId);
    
    return this.http.post<any>(this.baseUrl, dto, {
      params,
      headers: this.getHeaders()
    }).pipe(
      tap(response => console.log('Réponse backend:', response)),
      map(item => this.mapAudit(item))
    );
  }

  updateAudit(id: string, dto: CreateAuditDto): Observable<Audit> {
    console.log('=== updateAudit ===');
    console.log('ID:', id);
    console.log('DTO:', dto);
    const params = new HttpParams().set('organisationId', this.getOrgId());
    return this.http.put<any>(`${this.baseUrl}/${id}`, dto, {
      params,  
      headers: this.getHeaders()
    }).pipe(
      tap(response => console.log('Réponse update:', response)),
      map(item => this.mapAudit(item))
    );
  }

  deleteAudit(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  // ===========================
  // 🔵 EVALUATIONS
  // ===========================
  addEvaluation(auditId: string, dto: CreateEvaluationDto): Observable<Evaluation> {
    return this.http.post<any>(`${this.baseUrl}/${auditId}/evaluations`, dto, {
      headers: this.getHeaders()
    }).pipe(
      map(item => this.mapEvaluation(item))
    );
  }

  getEvaluations(auditId: string): Observable<Evaluation[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${auditId}/evaluations`, {
      headers: this.getHeaders()
    }).pipe(
      map(list => list.map(item => this.mapEvaluation(item)))
    );
  }

  // ===========================
  // 🔵 DROPDOWNS
  // ===========================
  getProcessus(): Observable<{ id: string; code: string; nom: string }[]> {
    const params = new HttpParams().set('organisationId', this.getOrgId());

    return this.http.get<any[]>(this.processusUrl, {
      params,
      headers: this.getHeaders()
    }).pipe(
      tap(data => console.log('API processus:', data)),
      map(list => list.map(p => ({
        id: p.id || p.Id,
        code: p.code || p.Code,
        nom: p.nom || p.Nom
      })))
    );
  }

  getResponsables(): Observable<{ id: number; nom: string }[]> {
    const params = new HttpParams().set('organisationId', this.getOrgId());

    return this.http.get<any[]>(`${this.baseUrl}/responsables`, {
      params,
      headers: this.getHeaders()
    }).pipe(
      map(list => list.map(r => ({
        id: r.id || r.Id,
        nom: r.nom || r.Nom
      })))
    );
  }

  getUsers(): Observable<{ id: number; nom: string }[]> {
    const params = new HttpParams().set('organisationId', this.getOrgId());

    return this.http.get<any[]>(`${this.baseUrl}/users`, {
      params,
      headers: this.getHeaders()
    }).pipe(
      map(list => list.map(u => ({
        id: u.id || u.Id,
        nom: u.nom || u.Nom
      })))
    );
  }

  // ===========================
  // 🔵 MAPPERS
  // ===========================
  private mapAudit(item: any): Audit {
    return {
      id: item.id || item.Id,
      organisationId: item.organisationId || item.OrganisationId,
      processusId: item.processusId || item.ProcessusId,
      processusCode: item.processusCode || item.ProcessusCode,
      processusNom: item.processusNom || item.ProcessusNom,
      nom: item.nom || item.Nom,
      description: item.description || item.Description,
      type: (item.type || item.Type || 'DOCUMENTAIRE') as AuditType,
      frequence: (item.frequence || item.Frequence || 'ANNUEL') as AuditFrequence,
      responsableId: item.responsableId ?? item.ResponsableId,
      responsableNom: item.responsableNom || item.ResponsableNom,
      actif: item.actif ?? item.Actif ?? true,
      dateCreation: item.dateCreation || item.DateCreation,
      dateModification: item.dateModification || item.DateModification,
      derniereEvaluation: item.derniereEvaluation || item.DerniereEvaluation
        ? this.mapEvaluation(item.derniereEvaluation || item.DerniereEvaluation)
        : undefined,
      evaluations: (item.evaluations || item.Evaluations || []).map((e: any) => this.mapEvaluation(e))
    };
  }

  private mapEvaluation(item: any): Evaluation {
    return {
      id: item.id || item.Id,
      pointControleId: item.pointControleId || item.PointControleId,
      dateEvaluation: item.dateEvaluation || item.DateEvaluation,
      conforme: item.conforme ?? item.Conforme ?? true,
      commentaire: item.commentaire || item.Commentaire,
      evalueParId: item.evalueParId ?? item.EvalueParId,
      evalueParNom: item.evalueParNom || item.EvalueParNom,
      dateCreation: item.dateCreation || item.DateCreation
    };
  }
}
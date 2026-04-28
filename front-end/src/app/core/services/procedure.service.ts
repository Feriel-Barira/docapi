import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Procedure, CreateProcedureDto } from '../../shared/models/procedure.model';
import { AuthService } from './auth.service';
@Injectable({ providedIn: 'root' })
export class ProcedureService {

  private readonly API = `${environment.apiUrl}/Procedure`;

  constructor(private http: HttpClient, private authService: AuthService) {}

  // ── Mapper backend → Procedure ───────────────────────────────────────
  private mapOne(p: any): Procedure {
  return {
    id:                 p.Id               ?? p.id               ?? '',
    organisationId:     p.OrganisationId   ?? p.organisationId   ?? '',
    processusId:        p.ProcessusId      ?? p.processusId      ?? '',  // ← déjà correct
    code:               p.Code             ?? p.code             ?? '',
    titre:              p.Titre            ?? p.titre            ?? '',
    objectif:           p.Objectif         ?? p.objectif         ?? '',
    domaineApplication: p.DomaineApplication ?? p.domaineApplication ?? '',
    description:        p.Description      ?? p.description      ?? '',
    responsableId:      String(p.ResponsableId ?? p.responsableId ?? ''),
    statut:             p.Statut           ?? p.statut           ?? 'ACTIF',
    dateCreation:       p.DateCreation     ?? p.dateCreation     ?? '',
    processus:          p.processus ?? undefined,
    responsable: p.responsable ? {
  id:       String(p.responsable.id || p.responsable.Id),
  nom:      p.responsable.nom      || p.responsable.Nom      || '',
  prenom:   p.responsable.prenom   || p.responsable.Prenom   || '',
  fonction: p.responsable.fonction || p.responsable.Fonction || ''
} : undefined,
    instructions:      (p.instructions ?? p.Instructions ?? []).map((i: any) => ({
      id:             i.Id             ?? i.id             ?? '',
      organisationId: i.OrganisationId ?? i.organisationId ?? '',
      procedureId:    i.ProcedureId    ?? i.procedureId    ?? '',
      code:           i.Code           ?? i.code           ?? '',
      titre:          i.Titre          ?? i.titre          ?? '',
      description:    i.Description    ?? i.description    ?? '',
      statut:         i.Statut         ?? i.statut         ?? 'ACTIF',
      dateCreation:   i.DateCreation   ?? i.dateCreation   ?? '',
    })),
    documentsAssocies: (p.documentsAssocies ?? p.DocumentsAssocies ?? []).map((d: any) => ({
      id:           d.Id           ?? d.id           ?? '',
      code:         d.Code         ?? d.code         ?? '',
      titre:        d.Titre        ?? d.titre        ?? '',
      typeDocument: d.TypeDocument ?? d.typeDocument ?? 'REFERENCE',
      version:      d.Version      ?? d.version,
    })),
   
  };
}

  // ── GET ALL ──────────────────────────────────────────────────────────
 getProcedures(processusId?: string): Observable<Procedure[]> {
  const orgId = localStorage.getItem('auth_organisation_id') ?? '';
  let params = new HttpParams();
  if (orgId)       params = params.set('organisationId', orgId);
  if (processusId) params = params.set('processusId', processusId);

  return this.http.get<any[]>(this.API, { params }).pipe(
    map(list => list.map(p => this.mapOne(p))),
    catchError((err) => {
      console.error('Erreur getProcedures:', err);
      return of([]);  // ← tableau vide au lieu des données statiques
    })
  );
}

  // ── GET BY ID ────────────────────────────────────────────────────────
  getProcedure(id: string): Observable<Procedure> {
    return this.http.get<any>(`${this.API}/${id}`).pipe(
      map(p => this.mapOne(p))
    );
  }

  // ── CREATE ───────────────────────────────────────────────────────────
createProcedure(dto: any): Observable<Procedure> {
  const orgId = localStorage.getItem('auth_organisation_id') ?? '';
  const processusId = dto.processusId || '';
  return this.http.post<any>(
    `${this.API}/${processusId}?organisationId=${orgId}`, dto
  ).pipe(
    map(p => this.mapOne(p)),
    catchError((err) => {
      console.error('Erreur createProcedure:', err);
      return of({} as Procedure);
    })
  );
}

  // ── UPDATE ───────────────────────────────────────────────────────────
  updateProcedure(id: string, dto: Partial<CreateProcedureDto>): Observable<Procedure> {
  return this.http.put<any>(`${this.API}/${id}`, dto).pipe(
    map(p => this.mapOne(p))
  );
}

  // ── DELETE ───────────────────────────────────────────────────────────
  deleteProcedure(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API}/${id}`);
  }

  checkProcedureCode(code: string, excludeId?: string): Observable<boolean> {
  const organisationId = this.authService.getOrganisationId();
  let params = new HttpParams()
    .set('code', code)
    .set('organisationId', organisationId);
  if (excludeId) {
    params = params.set('excludeId', excludeId);
  }
  return this.http.get<{ exists: boolean }>(`${environment.apiUrl}/Procedure/check-code`, { params })
    .pipe(map(res => res.exists));
}

}
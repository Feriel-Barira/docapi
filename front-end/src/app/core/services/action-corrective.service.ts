import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ActionCorrectiveDto {
  id: string;
  type: string;
  description: string;
  statut: string;      // PLANIFIEE | EN_COURS | REALISEE | VERIFIEE
  dateEcheance: string;
  dateRealisation?: string;
  dateVerification?: string;
  commentaireRealisation?: string;
  commentaireVerification?: string;
  dateCreation: string;
  responsable?: { id: number; nomComplet: string; email: string; };
}

@Injectable({ providedIn: 'root' })
export class ActionCorrectiveService {

  private readonly API = `${environment.apiUrl}/ActionCorrective`;

  constructor(private http: HttpClient) {}

  getByNonConformite(nonConformiteId: string): Observable<ActionCorrectiveDto[]> {
    return this.http.get<ActionCorrectiveDto[]>(`${this.API}/nonconformite/${nonConformiteId}`);
  }

  getById(id: string): Observable<ActionCorrectiveDto> {
    return this.http.get<ActionCorrectiveDto>(`${this.API}/${id}`);
  }

  getEcheanceProche(organisationId: string, joursAlerte = 7): Observable<ActionCorrectiveDto[]> {
    return this.http.get<ActionCorrectiveDto[]>(`${this.API}/echeance-proche`, {
      params: new HttpParams()
        .set('organisationId', organisationId)
        .set('joursAlerte', joursAlerte.toString())
    });
  }

  create(nonConformiteId: string, dto: {
    type: string; description: string; responsableId: number; dateEcheance: string;
  }): Observable<ActionCorrectiveDto> {
    return this.http.post<ActionCorrectiveDto>(`${this.API}/nonconformite/${nonConformiteId}`, dto);
  }

  update(id: string, dto: Partial<ActionCorrectiveDto>): Observable<ActionCorrectiveDto> {
    return this.http.put<ActionCorrectiveDto>(`${this.API}/${id}`, dto);
  }

  realiser(id: string, commentaire: string): Observable<ActionCorrectiveDto> {
    return this.http.post<ActionCorrectiveDto>(
      `${this.API}/${id}/realiser`,
      JSON.stringify(commentaire),
      { headers: { 'Content-Type': 'application/json' } }
    );
  }

  verifier(id: string, efficace: boolean, commentaire?: string): Observable<ActionCorrectiveDto> {
    return this.http.post<ActionCorrectiveDto>(`${this.API}/${id}/verifier`, { efficace, commentaire });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API}/${id}`);
  }
attacherPreuve(acId: string, enregId: string): Observable<void> {
  return this.http.put<void>(`${this.API}/${acId}/preuve`, JSON.stringify(enregId), {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  });
}

detacherPreuve(acId: string): Observable<void> {
  return this.http.delete<void>(`${this.API}/${acId}/preuve`);
}
}
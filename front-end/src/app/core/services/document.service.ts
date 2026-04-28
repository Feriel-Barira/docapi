// src/app/core/services/document.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { DocumentISO, VersionDocument, CreateDocumentDto, DocTypeDocument, DocStatut } from '../../shared/models/document.model';

@Injectable({ providedIn: 'root' })
export class DocumentService {

  private readonly API = `${environment.apiUrl}/Document`;

  constructor(private http: HttpClient) {}

  private formatDate(date: any): string {
    if (!date) return '';
    if (typeof date === 'string') {
      if (date.includes('/')) return date;
      return date.split('T')[0];
    }
    return '';
  }

  private mapVersion(v: any): VersionDocument {
    return {
      id:                  v.id ?? v.Id ?? '',
      documentId:          v.documentId ?? v.DocumentId ?? '',
      numeroVersion:       v.numeroVersion ?? v.NumeroVersion ?? '',
      statut:              v.statut ?? v.Statut ?? 'BROUILLON',
      commentaireRevision: v.commentaireRevision ?? v.CommentaireRevision ?? '',
      fichierPath:         v.fichierPath ?? v.FichierPath,
      etabliParId:         v.etabliParId ?? v.EtabliParId,
      verifieParId:        v.verifieParId ?? v.VerifieParId,
      valideParId:         v.valideParId ?? v.ValideParId,
      etabliParNom:        v.etabliPar?.nomComplet ?? v.EtabliPar?.nomComplet,
      verifieParNom:       v.verifiePar?.nomComplet ?? v.VerifiePar?.nomComplet,
      valideParNom:        v.validePar?.nomComplet ?? v.ValidePar?.nomComplet,
      dateEtablissement:   this.formatDate(v.dateEtablissement ?? v.DateEtablissement),
      dateVerification:    this.formatDate(v.dateVerification ?? v.DateVerification),
      dateValidation:      this.formatDate(v.dateValidation ?? v.DateValidation),
      dateMiseEnVigueur:   this.formatDate(v.dateMiseEnVigueur ?? v.DateMiseEnVigueur),
    };
  }

  private mapOne(d: any): DocumentISO {
    const allVersions = (d.versions ?? d.Versions ?? []).map((v: any) => this.mapVersion(v));
    
    const sortedVersions = allVersions.sort((a: VersionDocument, b: VersionDocument) => {
      const aNum = parseInt(a.numeroVersion?.replace(/[^0-9]/g, '') || '0');
      const bNum = parseInt(b.numeroVersion?.replace(/[^0-9]/g, '') || '0');
      return bNum - aNum;
    });
    
    const lastV = sortedVersions[0];

    const getInitiales = (nom: string | undefined): string => {
      if (!nom) return '?';
      const parts = nom.trim().split(' ').filter(p => p.length > 0);
      if (parts.length >= 2) {
        return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
      }
      return nom.substring(0, 2).toUpperCase();
    };

    const getCouleur = (nom: string | undefined): string => {
      if (!nom) return '#6B7280';
      const couleurs = ['#1A5276', '#9B1C1C', '#1B4F72', '#7D6608', '#1E8449', '#6C3483', '#A04000'];
      let hash = 0;
      for (let i = 0; i < nom.length; i++) {
        hash = ((hash << 5) - hash) + nom.charCodeAt(i);
        hash |= 0;
      }
      return couleurs[Math.abs(hash) % couleurs.length];
    };

    let dateMiseAJour = '—';
    if (lastV?.dateValidation) {
      dateMiseAJour = lastV.dateValidation;
    } else if (lastV?.dateEtablissement) {
      dateMiseAJour = lastV.dateEtablissement;
    }

    return {
      id:                     d.id ?? d.Id ?? '',
      organisationId:         d.organisationId ?? d.OrganisationId,
      code:                   d.code ?? d.Code ?? '',
      titre:                  d.titre ?? d.Titre ?? '',
      typeDocument:           (d.typeDocument ?? d.TypeDocument ?? 'REFERENCE') as DocTypeDocument,
      processusId:            d.processus?.id ?? d.Processus?.id ?? d.processusId ?? d.ProcessusId,
      processusCode:          d.processus?.code ?? d.Processus?.code,
      description:            d.description ?? d.Description ?? '',
      actif:                  d.actif ?? d.Actif ?? true,
      dateCreation:           this.formatDate(d.dateCreation ?? d.DateCreation),
      derniereVersion:        lastV?.numeroVersion,
      derniereVersionStatut:  lastV?.statut as DocStatut,
      dateMiseAJour:          dateMiseAJour,
      responsableId:          lastV?.etabliParId?.toString(),
      responsableNom:         lastV?.etabliParNom,
      responsableInitiales:   getInitiales(lastV?.etabliParNom),
      responsableCouleur:     getCouleur(lastV?.etabliParNom),
      versions:               sortedVersions,
      proceduresAssocieesIds: d.proceduresAssocieesIds ?? [],
    };
  }

  // ── API calls ────────────────────────────────────────────────────────
  getDocuments(processusId?: string): Observable<DocumentISO[]> {
    const orgId = localStorage.getItem('auth_organisation_id') ?? '';
    let params = new HttpParams();
    if (orgId) params = params.set('organisationId', orgId);
    if (processusId) params = params.set('processusId', processusId);
    
    return this.http.get<any[]>(this.API, { params }).pipe(
      map(list => list.map(d => this.mapOne(d))),
      catchError(error => {
        console.error('❌ Erreur getDocuments:', error);
        return throwError(() => error);
      })
    );
  }

  addVersion(documentId: string, dto: any): Observable<VersionDocument> {
    return this.http.post<any>(`${this.API}/${documentId}/versions`, dto).pipe(
      map(v => this.mapVersion(v)),
      catchError(error => {
        console.error('❌ Erreur addVersion:', error);
        return throwError(() => error);
      })
    );
  }

  getDocument(id: string): Observable<DocumentISO> {
    return this.http.get<any>(`${this.API}/${id}`).pipe(
      map(d => this.mapOne(d)),
      catchError(error => {
        console.error(`❌ Erreur getDocument ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  getVersions(documentId: string): Observable<VersionDocument[]> {
    return this.http.get<any[]>(`${this.API}/${documentId}/versions`).pipe(
      map(list => list.map(v => this.mapVersion(v))),
      catchError(error => {
        console.error('❌ Erreur getVersions:', error);
        return throwError(() => error);
      })
    );
  }

  createDocument(dto: CreateDocumentDto): Observable<DocumentISO> {
    const orgId = localStorage.getItem('auth_organisation_id') ?? '';
    return this.http.post<any>(`${this.API}?organisationId=${orgId}`, dto).pipe(
      map(d => this.mapOne(d)),
      catchError(error => {
        console.error('❌ Erreur createDocument:', error);
        return throwError(() => error);
      })
    );
  }

  updateDocument(id: string, dto: Partial<CreateDocumentDto>): Observable<DocumentISO> {
    return this.http.put<any>(`${this.API}/${id}`, dto).pipe(
      map(d => this.mapOne(d)),
      catchError(error => {
        console.error('❌ Erreur updateDocument:', error);
        return throwError(() => error);
      })
    );
  }

  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API}/${id}`).pipe(
      catchError(error => {
        console.error('❌ Erreur deleteDocument:', error);
        return throwError(() => error);
      })
    );
  }

  soumettreVersion(versionId: string): Observable<VersionDocument> {
    return this.http.post<any>(`${this.API}/versions/${versionId}/soumettre`, {}).pipe(
      map(v => this.mapVersion(v)),
      catchError(error => {
        console.error('❌ Erreur soumettreVersion:', error);
        return throwError(() => error);
      })
    );
  }

  validerVersion(versionId: string, commentaire?: string): Observable<VersionDocument> {
    return this.http.post<any>(`${this.API}/versions/${versionId}/valider`, commentaire ?? '').pipe(
      map(v => this.mapVersion(v)),
      catchError(error => {
        console.error('❌ Erreur validerVersion:', error);
        return throwError(() => error);
      })
    );
  }

  rejeterVersion(versionId: string, commentaire: string): Observable<VersionDocument> {
    return this.http.post<any>(`${this.API}/versions/${versionId}/rejeter`, commentaire).pipe(
      map(v => this.mapVersion(v)),
      catchError(error => {
        console.error('❌ Erreur rejeterVersion:', error);
        return throwError(() => error);
      })
    );
  }

  archiverVersion(versionId: string): Observable<VersionDocument> {
    return this.http.post<any>(`${this.API}/versions/${versionId}/archiver`, {}).pipe(
      map(v => this.mapVersion(v)),
      catchError(error => {
        console.error('❌ Erreur archiverVersion:', error);
        return throwError(() => error);
      })
    );
  }
}
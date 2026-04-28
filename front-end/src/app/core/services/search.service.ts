import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SearchResultDto {
  terme: string;
  totalResultats: number;
  processus: SearchItemDto[];
  procedures: SearchItemDto[];
  documents: SearchItemDto[];
  nonConformites: SearchItemDto[];
  indicateurs: SearchItemDto[];
}

export interface SearchItemDto {
  id: string;
  type: string;
  code: string;
  titre: string;
  description?: string;
  statut?: string;
  dateCreation: string;
}

@Injectable({ providedIn: 'root' })
export class SearchService {

  private readonly API = `${environment.apiUrl}/Search`;

  constructor(private http: HttpClient) {}

  search(terme: string, organisationId: string, options?: {
    includeProcessus?: boolean;
    includeProcedures?: boolean;
    includeDocuments?: boolean;
    includeNonConformites?: boolean;
    includeIndicateurs?: boolean;
  }): Observable<SearchResultDto> {
    let params = new HttpParams()
      .set('terme', terme)
      .set('organisationId', organisationId);

    if (options?.includeProcessus      !== undefined) params = params.set('includeProcessus',      options.includeProcessus.toString());
    if (options?.includeProcedures     !== undefined) params = params.set('includeProcedures',     options.includeProcedures.toString());
    if (options?.includeDocuments      !== undefined) params = params.set('includeDocuments',      options.includeDocuments.toString());
    if (options?.includeNonConformites !== undefined) params = params.set('includeNonConformites', options.includeNonConformites.toString());
    if (options?.includeIndicateurs    !== undefined) params = params.set('includeIndicateurs',    options.includeIndicateurs.toString());

    return this.http.get<SearchResultDto>(this.API, { params });
  }
}
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { EnregistrementDto } from 'src/app/shared/models/enregistrement.model';

@Injectable({ providedIn: 'root' })
export class EnregistrementService {
  private api = `${environment.apiUrl}/enregistrement`;

  constructor(private http: HttpClient) {}

  getAll(processusId?: string): Observable<EnregistrementDto[]> {
    let params = new HttpParams();
    if (processusId) params = params.set('processusId', processusId);
    return this.http.get<EnregistrementDto[]>(this.api, { params });
  }

  upload(formData: FormData): Observable<EnregistrementDto> {
    return this.http.post<EnregistrementDto>(this.api, formData);
  }

  download(id: string): Observable<Blob> {
    return this.http.get(`${this.api}/download/${id}`, { responseType: 'blob' });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}
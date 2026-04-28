import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly API = `${environment.apiUrl}/Admin/users`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token');
    return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
  }

  getUsers(): Observable<any[]> {
  const orgId = localStorage.getItem('auth_organisation_id') || '';
  // Appel sans headers Authorization
  return this.http.get<any[]>(`${this.API}?organisationId=${orgId}`);
}

  getUserById(id: number): Observable<any> {
    return this.http.get<any>(`${this.API}/${id}`, {
      headers: this.getHeaders()
    });
  }

 createUser(user: any): Observable<any> {
  const orgId = localStorage.getItem('auth_organisation_id') || '';
  return this.http.post<any>(`${this.API}?organisationId=${orgId}`, user, {
    headers: this.getHeaders()
  });
}

  updateUser(user: any): Observable<any> {
  const orgId = localStorage.getItem('auth_organisation_id') || '';
  return this.http.put<any>(`${this.API}/${user.id}?organisationId=${orgId}`, user, {
    headers: this.getHeaders()
  });
}
deleteUser(id: number): Observable<void> {
  const orgId = localStorage.getItem('auth_organisation_id') || '';
  return this.http.delete<void>(`${this.API}/${id}?organisationId=${orgId}`, {
    headers: this.getHeaders()
  });
}
}
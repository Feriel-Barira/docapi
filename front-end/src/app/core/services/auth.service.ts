import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { LoginRequest, AuthResponse, AuthUser, RefreshTokenRequest } from '../../shared/models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly TOKEN_KEY         = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'auth_refresh_token';
  private readonly USER_KEY          = 'auth_user';
  private readonly ORG_KEY           = 'auth_organisation_id';
  private readonly ROLE_KEY          = 'auth_role';

  // ← OrganisationId de démo inséré dans CreateDatabase_fixed.sql
  private readonly DEMO_ORG_ID = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';

  private readonly API_URL = `${environment.apiUrl}/auth`;

  private currentUserSubject = new BehaviorSubject<AuthUser | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/login`, {
      usernameOrEmail: credentials.usernameOrEmail,
      password:        credentials.password
    }).pipe(
      tap(response => this.storeSession(response, credentials.usernameOrEmail)),
      catchError(error => {
        console.error('Erreur login:', error);
        return throwError(() => new Error('Nom d\'utilisateur ou mot de passe incorrect'));
      })
    );
  }

  refresh(): Observable<AuthResponse> {
    const body: RefreshTokenRequest = {
      token:        this.getToken() ?? '',
      refreshToken: this.getRefreshToken() ?? ''
    };
    return this.http.post<AuthResponse>(`${this.API_URL}/refresh`, body).pipe(
      tap(response => {
        localStorage.setItem(this.TOKEN_KEY,         response.token);
        localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.API_URL}/logout`, JSON.stringify(refreshToken), {
        headers: { 'Content-Type': 'application/json' }
      }).subscribe({ error: () => {} });
    }
    this.clearSession();
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean       { return !!this.getToken(); }
  getToken(): string | null        { return localStorage.getItem(this.TOKEN_KEY); }
  getRefreshToken(): string | null { return localStorage.getItem(this.REFRESH_TOKEN_KEY); }
  getOrganisationId(): string      { return localStorage.getItem(this.ORG_KEY) ?? ''; }
  getRole(): string                { return localStorage.getItem(this.ROLE_KEY) ?? ''; }
  getCurrentUser(): AuthUser | null { return this.currentUserSubject.value; }

  isAdmin(): boolean   { return this.getRole() === 'ADMIN_ORG'; }
isManager(): boolean { return this.getRole() === 'RESPONSABLE_SMQ'; }
isAuditeur(): boolean { return this.getRole() === 'AUDITEUR'; }
isUtilisateur(): boolean { return this.getRole() === 'UTILISATEUR'; }

  private storeSession(response: AuthResponse, usernameOrEmail: string): void {
    localStorage.setItem(this.TOKEN_KEY,         response.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.ROLE_KEY,          response.role);

    // ← FIX : stocker l'organisationId
    const orgId = (response as any).organisationId ?? this.DEMO_ORG_ID;
    localStorage.setItem(this.ORG_KEY, orgId);

    const user: AuthUser = {
      id:       (response as any).userId ?? 0,
      username: response.username ?? usernameOrEmail,
      email:    usernameOrEmail,
      role:     response.role,
      fonction: (response as any).fonction ?? '' ,
      nom:      (response as any).nom ?? '',      
      prenom:   (response as any).prenom ?? '' 
    };
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.ORG_KEY);
    localStorage.removeItem(this.ROLE_KEY);
    this.currentUserSubject.next(null);
  }

  private getUserFromStorage(): AuthUser | null {
    try {
      const str = localStorage.getItem(this.USER_KEY);
      return str ? JSON.parse(str) : null;
    } catch { return null; }
  }
  getUserRole(): string { 
  return this.getRole(); 
}

hasRole(roles: string[]): boolean { 
  return roles.includes(this.getRole()); 
}
}
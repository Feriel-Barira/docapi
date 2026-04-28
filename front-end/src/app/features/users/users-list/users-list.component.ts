import { Component, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { NotificationService } from '../../../core/services/notification.service';

// Interface correspondant exactement aux données renvoyées par le backend (PascalCase)
interface User {
  Id: number;
  Username: string;
  Email: string;
  Nom: string;
  Prenom: string;
  RoleGlobal: string;
  Fonction: string;
  IsActive: boolean;
}

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent implements OnInit, OnDestroy {
  
  users: User[] = [];
  filteredUsers: User[] = [];
  
  searchTerm = '';
  roleFilter = '';
  currentPage = 1;
  pageSize = 10;
  
  showFormModal = false;
  showDeleteModal = false;
  
  selectedUser: User | null = null;
  isEdit = false;
  loading = false;
  
  form = {
    id: 0,
    username: '',
    email: '',
    nom: '',
    prenom: '',
    password: '',
    roleGlobal: 'UTILISATEUR',
    fonction: '',
    isActive: true
  };
  
  constructor(
    private http: HttpClient,
    private renderer: Renderer2,
    private notification: NotificationService
  ) {}
  
  currentUserId: number = 0;

ngOnInit(): void {
  this.currentUserId = this.getUserIdFromToken();
  console.log('🔑 currentUserId depuis token :', this.currentUserId);
  this.loadUsers();
}
  ngOnDestroy(): void {
    this.renderer.removeClass(document.body, 'modal-open');
  }
  
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token');
    return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
  }
  
  private getOrgId(): string {
    return localStorage.getItem('auth_organisation_id') || '';
  }
 private getUserIdFromToken(): number {
  const token = localStorage.getItem('auth_token');
  if (!token) return 0;
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const payload = JSON.parse(atob(base64));
    console.log('🔓 Payload complet du token :', payload);
    // Essaye plusieurs clés possibles
    const id = payload.nameid || payload.Id || payload.userId || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || 0;
    console.log('🔑 ID extrait :', id);
    return Number(id);
  } catch (e) {
    console.error('Erreur décodage token', e);
    return 0;
  }
}
  
  loadUsers(): void {
    this.loading = true;
    const orgId = this.getOrgId();
    
    this.http.get<User[]>(`${environment.apiUrl}/Admin/users?organisationId=${orgId}`, {
      headers: this.getHeaders()
    }).subscribe({
      next: (data) => {
        console.log('📋 Données reçues :', data);
        this.users = data;
        console.log('📋 Utilisateurs avec ID :', this.users.map(u => ({ Id: u.Id, Nom: u.Nom })));
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error('Erreur chargement users:', err);
        this.loading = false;
      }
    });
  }
  
  applyFilters(): void {
    let filtered = [...this.users];
    
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(u =>
        u.Prenom?.toLowerCase().includes(term) ||
        u.Nom?.toLowerCase().includes(term) ||
        u.Email?.toLowerCase().includes(term) ||
        u.Username?.toLowerCase().includes(term)
      );
    }
    
    if (this.roleFilter) {
      filtered = filtered.filter(u => u.RoleGlobal === this.roleFilter);
    }
    
    this.filteredUsers = filtered;
    this.currentPage = 1;
  }
  
  onSearchChange(): void {
    this.applyFilters();
  }
  
  onRoleFilterChange(): void {
    this.applyFilters();
  }
  
  get paginatedUsers(): User[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredUsers.slice(start, start + this.pageSize);
  }
  
  get totalPages(): number {
    return Math.ceil(this.filteredUsers.length / this.pageSize);
  }
  
  get lastIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredUsers.length);
  }
  
  pagesArray(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }
  
  getRoleLabel(role: string): string {
    switch (role) {
      case 'ADMIN_ORG': return 'Administrateur';
      case 'RESPONSABLE_SMQ': return 'Responsable SMQ';
      case 'AUDITEUR': return 'Auditeur';
      default: return 'Utilisateur';
    }
  }
  
  getRoleClass(role: string): string {
    switch (role) {
      case 'ADMIN_ORG': return 'admin';
      case 'RESPONSABLE_SMQ': return 'responsable';
      case 'AUDITEUR': return 'auditeur';
      default: return 'utilisateur';
    }
  }
  
  private openModal(): void {
    this.renderer.addClass(document.body, 'modal-open');
  }
  
  private closeModal(): void {
    this.renderer.removeClass(document.body, 'modal-open');
  }
  
  openCreate(): void {
    this.isEdit = false;
    this.form = {
      id: 0,
      username: '',
      email: '',
      nom: '',
      prenom: '',
      password: '',
      roleGlobal: 'UTILISATEUR',
      fonction: '',
      isActive: true
    };
    this.showFormModal = true;
    this.openModal();
  }
  
 openEdit(user: User): void {
  console.log('🔍 Utilisateur à modifier :', user);
  console.log('🔑 user.Id =', user.Id, typeof user.Id);
  this.isEdit = true;
  this.selectedUser = user;
  this.form = {
    id: user.Id,          // doit être un nombre (ex: 3)
    username: user.Username,
    email: user.Email,
    nom: user.Nom,
    prenom: user.Prenom,
    password: '',
    roleGlobal: user.RoleGlobal,
    fonction: user.Fonction,
    isActive: user.IsActive
  };
  this.showFormModal = true;
  this.openModal();
}
  
  closeForm(): void {
    this.showFormModal = false;
    this.selectedUser = null;
    this.closeModal();
  }
  
  saveUser(): void {
  if (!this.form.username || !this.form.email || !this.form.nom || !this.form.prenom) {
    this.notification.showError('Veuillez remplir tous les champs obligatoires');
    return;
  }

  const orgId = this.getOrgId();
  const url = this.isEdit
    ? `${environment.apiUrl}/Admin/users/${this.form.id}?organisationId=${orgId}`
    : `${environment.apiUrl}/Admin/users?organisationId=${orgId}`;

  const payload: any = {
    username: this.form.username,
    email: this.form.email,
    nom: this.form.nom,
    prenom: this.form.prenom,
    roleGlobal: this.form.roleGlobal,
    fonction: this.form.fonction,
    isActive: this.form.isActive
  };

  if (!this.isEdit && this.form.password) {
    payload.password = this.form.password;
  }

  const request = this.isEdit
    ? this.http.put(url, payload, { headers: this.getHeaders() })
    : this.http.post(url, payload, { headers: this.getHeaders() });

  request.subscribe({
    next: () => {
      this.notification.showSuccess(this.isEdit ? 'Utilisateur modifié' : 'Utilisateur créé');
      this.closeForm();
      this.loadUsers();
    },
    error: (err) => {
      console.error('Erreur:', err);
      this.notification.showError(err.error?.message || 'Erreur lors de l\'opération');
    }
  });
}
  
  openDelete(user: User): void {
  console.log('🗑️ openDelete - user :', user);
  console.log('🗑️ user.Id :', user.Id);
  this.selectedUser = user;
  this.showDeleteModal = true;
  this.openModal();
}
  
  closeDelete(): void {
    this.showDeleteModal = false;
    this.selectedUser = null;
    this.closeModal();
  }
  
 confirmDelete(): void {
  if (!this.selectedUser) return;

  const orgId = this.getOrgId();
  const url = `${environment.apiUrl}/Admin/users/${this.selectedUser.Id}?organisationId=${orgId}`;
  
  console.log('🗑️ URL de suppression :', url);  // Vérifier l’URL
  console.log('🗑️ selectedUser :', this.selectedUser);

  this.http.delete(url, { headers: this.getHeaders() }).subscribe({
    next: () => {
      this.notification.showSuccess('Utilisateur supprimé');
      this.closeDelete();
      this.loadUsers();
    },
    error: (err) => {
      console.error('Erreur:', err);
      this.notification.showError('Erreur lors de la suppression');
    }
  });
}
}
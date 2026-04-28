import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-parametres',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './parametres.component.html',
  styleUrls: ['./parametres.component.scss']
})
export class ParametresComponent implements OnInit {

  logs: any[] = [];
  allLogs: any[] = [];
  loading = false;
  totalLogs = 0;

  currentPage = 1;
  pageSize = 8;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token') || '';
    return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
  }

  loadLogs(): void {
    this.loading = true;
    const url = `${environment.apiUrl}/AuditLog/recent?count=200`;

    this.http.get<any[]>(url, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        this.allLogs = data || [];
        this.totalLogs = this.allLogs.length;
        this.currentPage = 1;
        this.applyPagination();
        this.loading = false;
      },
      error: (err) => {
        console.error('Erreur chargement logs:', err);
        this.loading = false;
      }
    });
  }

  applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    this.logs = this.allLogs.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.totalLogs / this.pageSize);
  }

  get lastIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalLogs);
  }

  get firstIndex(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyPagination();
  }

  pagesArray(): number[] {
    const total = this.totalPages;
    const current = this.currentPage;
    const pages: number[] = [];
    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);
    if (end - start < 4) {
      if (start === 1) end = Math.min(total, start + 4);
      else start = Math.max(1, end - 4);
    }
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  getActionIcon(action: string): string {
    if (action.startsWith('CREATE')) return '➕';
    if (action.startsWith('UPDATE')) return '✏️';
    if (action.startsWith('DELETE')) return '🗑️';
    return '●';
  }

  getActionClass(action: string): string {
    if (action.startsWith('CREATE')) return 'action-create';
    if (action.startsWith('UPDATE')) return 'action-update';
    if (action.startsWith('DELETE')) return 'action-delete';
    return '';
  }

  getActionLabel(action: string): string {
    const labels: any = {
      'CREATE_USER': 'Création utilisateur',
      'UPDATE_USER': 'Modification utilisateur',
      'DELETE_USER': 'Suppression utilisateur',
      'CREATE_PROCESSUS': 'Création processus',
      'UPDATE_PROCESSUS': 'Modification processus',
      'DELETE_PROCESSUS': 'Suppression processus',
      'CREATE_PROCEDURE': 'Création procédure',
      'UPDATE_PROCEDURE': 'Modification procédure',
      'DELETE_PROCEDURE': 'Suppression procédure',
      'CREATE_DOCUMENT': 'Création document',
      'UPDATE_DOCUMENT': 'Modification document',
      'DELETE_DOCUMENT': 'Suppression document',
      'CREATE_NC': 'Création NC',
      'UPDATE_NC': 'Modification NC',
      'DELETE_NC': 'Suppression NC',
      'CREATE_AC': 'Création AC',
      'UPDATE_AC': 'Modification AC',
      'CREATE_INDICATEUR': 'Création indicateur',
      'UPDATE_INDICATEUR': 'Modification indicateur'
    };
    return labels[action] || action;
  }

  formatDate(date: string): string {
    if (!date) return '—';
    try {
      return new Date(date).toLocaleString('fr-FR');
    } catch { return date; }
  }
}
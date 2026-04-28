import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProcessusDto, ProcessType, ProcessStatut } from '../../../shared/models/process.model';
import { ProcessFormModalComponent } from '../process-form-modal/process-form-modal.component';
import { ProcessService } from '../../../core/services/process.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
@Component({
  selector: 'app-processus-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ProcessFormModalComponent],
  templateUrl: './processus-list.component.html',
  styleUrls: ['./processus-list.component.scss']
})
export class ProcessusListComponent implements OnInit {
  canEdit = false;  // ADMIN ou SMQ seulement

  processes: ProcessusDto[] = [];
  filteredProcesses: ProcessusDto[] = [];
  // Pagination pour la vue tableau
currentPage = 1;
pageSize = 6;
totalPages = 1;

  searchTerm = '';
  selectedType = '';
  viewMode: 'cards' | 'table' = 'cards';

  ProcessType = ProcessType;
  ProcessStatut = ProcessStatut;

  showModal = false;
  selectedProcess: ProcessusDto | null = null;

  showDeleteModal = false;
  processToDelete: ProcessusDto | null = null;

  constructor(private processService: ProcessService,private notification: NotificationService,private authService: AuthService ) {
     const role = this.authService.getRole();
    this.canEdit = role === 'ADMIN_ORG' || role === 'RESPONSABLE_SMQ';
  }

  // ✅ تحميل من backend
  ngOnInit(): void {
    this.loadProcesses();
  }

  loadProcesses(): void {
    this.processService.getProcesses().subscribe({
      next: (data) => {
        console.log('✅ Processus from backend:', data);
        this.processes = data;
        this.applyFilters();
      },
      error: (err) => {
        console.error('❌ Erreur chargement processus', err);
      }
    });
  }

  // ── Filtres ─────────────────────
  applyFilters(): void {
    let filtered = [...this.processes];

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(p =>
        (p.name ?? p.nom).toLowerCase().includes(term) ||
        p.code.toLowerCase().includes(term)
      );
    }

    if (this.selectedType) {
      filtered = filtered.filter(p => p.type === this.selectedType);
    }

    this.filteredProcesses = filtered;
    this.currentPage = 1;
  this.updatePagination();
  }

  onSearchChange(value: string): void {
    this.searchTerm = value;
    this.applyFilters();
  }

  onTypeFilterChange(type: string): void {
    this.selectedType = type;
    this.applyFilters();
  }

  // ── CREATE ─────────────────────
  openCreateModal(): void {
    this.selectedProcess = null;
    this.showModal = true;
  }

  // ── EDIT ─────────────────────
  openEditModal(process: ProcessusDto): void {
    this.selectedProcess = { ...process };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedProcess = null;
  }

  // ✅ SAVE via backend
 saveProcess(data: any): void {
  if (this.selectedProcess) {
    this.processService.updateProcess(this.selectedProcess.id, data).subscribe({
      next: () => {
        this.notification.showSuccess('Processus modifié avec succès !');
        this.loadProcesses();
        this.closeModal();
      },
      error: () => this.notification.showError('Erreur lors de la modification.')
    });
  } else {
    this.processService.createProcess(data).subscribe({
      next: () => {
        this.notification.showSuccess('Processus créé avec succès !');
        this.loadProcesses();
        this.closeModal();
      },
      error: () => this.notification.showError('Erreur lors de la création.')
    });
  }
}

  // ── DELETE ─────────────────────
  confirmDelete(process: ProcessusDto): void {
    this.processToDelete = process;
    this.showDeleteModal = true;
  }

 deleteProcess(): void {
  if (!this.processToDelete) return;
  this.processService.deleteProcess(this.processToDelete.id).subscribe({
    next: () => {
      this.notification.showSuccess('Processus supprimé avec succès !');
      this.loadProcesses();
      this.closeDeleteModal();
    },
    error: () => this.notification.showError('Erreur lors de la suppression.')
  });
}

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.processToDelete = null;
  }
  // ── Helpers HTML ─────────────────

getTypeClass(type: string): string {
  switch (type) {
    case 'PILOTAGE': return 'type-pilotage';
    case 'REALISATION': return 'type-realisation';
    case 'SUPPORT': return 'type-support';
    default: return '';
  }
}

getTypeBadgeClass(type: string): string {
  switch (type) {
    case 'PILOTAGE': return 'badge-pilotage';
    case 'REALISATION': return 'badge-realisation';
    case 'SUPPORT': return 'badge-support';
    default: return 'badge-default';
  }
}

getTypeLabel(type: string): string {
  switch (type) {
    case 'PILOTAGE': return 'Pilotage';
    case 'REALISATION': return 'Réalisation';
    case 'SUPPORT': return 'Support';
    default: return type;
  }
}
get paginatedProcesses(): ProcessusDto[] {
  const start = (this.currentPage - 1) * this.pageSize;
  return this.filteredProcesses.slice(start, start + this.pageSize);
}

get lastIndex(): number {
  const end = this.currentPage * this.pageSize;
  return end > this.filteredProcesses.length ? this.filteredProcesses.length : end;
}

pagesArray(): number[] {
  return Array(this.totalPages).fill(0).map((_, i) => i + 1);
}

updatePagination(): void {
  this.totalPages = Math.ceil(this.filteredProcesses.length / this.pageSize);
  if (this.currentPage > this.totalPages) this.currentPage = 1;
}
// Récupère les initiales (première lettre du prénom et du nom)
getPilotInitiales(process: ProcessusDto): string {
  const nomComplet = process.pilot || '';
  if (!nomComplet) return '?';
  const parts = nomComplet.trim().split(' ');
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
}

// Génère une couleur de fond cohérente pour chaque pilote
getPilotColor(process: ProcessusDto): string {
  const colors = ['#166534', '#6b21a8', '#7c2d12', '#155e75', '#92400e', '#1e3a5f'];
  let hash = 0;
  const name = process.pilot || '';
  for (let i = 0; i < name.length; i++) hash += name.charCodeAt(i);
  return colors[hash % colors.length];
}
}
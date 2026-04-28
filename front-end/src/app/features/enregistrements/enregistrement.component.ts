import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnregistrementService } from 'src/app/core/services/enregistrement.service';
import { ProcessService } from 'src/app/core/services/process.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { EnregistrementDto } from 'src/app/shared/models/enregistrement.model';
import { AuthService } from 'src/app/core/services/auth.service';
@Component({
  selector: 'app-enregistrement-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './enregistrement.component.html',
  styleUrls: ['./enregistrement.component.scss']
})
export class EnregistrementComponent implements OnInit {
  canEdit = false;
  enregistrements: EnregistrementDto[] = [];
 processusList: { id: string; code: string; nom: string }[] = [];
  selectedProcessusId = '';
  showUploadModal = false;
  uploadProcessusId = '';
  uploadFile: File | null = null;
  uploadDescription = '';
  showDeleteModal = false;
  enregistrementToDelete: EnregistrementDto | null = null;
  currentPage = 1;
  pageSize = 6;
  totalPages = 1;

  constructor(
    private enregService: EnregistrementService,
    private processService: ProcessService,
    private notification: NotificationService,
    private authService: AuthService 
  ) {}

  ngOnInit(): void {
    const role = this.authService.getRole();
    this.canEdit = role === 'ADMIN_ORG' || role === 'RESPONSABLE_SMQ';
    this.loadProcessus();
    this.loadEnregistrements();
  }

  loadProcessus(): void {
  this.processService.getProcesses().subscribe({
    next: (data) => {
      this.processusList = data.map(p => ({ 
        id: p.id, 
        code: p.code || '', 
        nom: p.nom || p.name || '' 
      }));
    },
    error: () => this.notification.showError('Erreur chargement processus')
  });
}

  loadEnregistrements(): void {
    this.enregService.getAll(this.selectedProcessusId || undefined).subscribe({
      next: (data) => {
        this.enregistrements = data;
        this.updatePagination();
      },
      error: () => this.notification.showError('Erreur chargement enregistrements')
    });
  }

  onProcessusChange(): void {
    this.currentPage = 1;
    this.loadEnregistrements();
  }

  openUploadModal(): void {
    this.uploadProcessusId = '';
    this.uploadFile = null;
    this.uploadDescription = '';
    this.showUploadModal = true;
  }

  closeModal(): void {
    this.showUploadModal = false;
  }

  onFileSelected(event: any): void {
    this.uploadFile = event.target.files[0];
  }

  upload(): void {
    if (!this.uploadFile || !this.uploadProcessusId) {
      this.notification.showError('Choisissez un processus et un fichier');
      return;
    }
    const formData = new FormData();
    const orgId = localStorage.getItem('auth_organisation_id') || '';
    formData.append('organisationId', orgId);
    formData.append('fichier', this.uploadFile);
    formData.append('processusId', this.uploadProcessusId);
    formData.append('description', this.uploadDescription);
    formData.append('typeEnregistrement', 'PREUVE_EXECUTION');
    formData.append('reference', new Date().toISOString().slice(0,19));

    this.enregService.upload(formData).subscribe({
      next: () => {
        this.notification.showSuccess('Preuve ajoutée');
        this.closeModal();
        this.loadEnregistrements();
      },
      error: () => this.notification.showError('Erreur upload')
    });
  }

  download(id: string, fileName: string): void {
    this.enregService.download(id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  delete(id: string): void {
    const toDelete = this.enregistrements.find(e => e.id === id);
    if (toDelete) {
      this.enregistrementToDelete = toDelete;
      this.showDeleteModal = true;
    }
  }

  confirmDelete(): void {
    if (!this.enregistrementToDelete) return;
    const id = this.enregistrementToDelete.id;
    this.enregService.delete(id).subscribe({
      next: () => {
        this.notification.showSuccess('Supprimé');
        this.loadEnregistrements();
        this.closeDeleteModal();
      },
      error: () => this.notification.showError('Erreur suppression')
    });
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.enregistrementToDelete = null;
  }

  get paginatedEnregistrements(): EnregistrementDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.enregistrements.slice(start, start + this.pageSize);
  }

  get lastIndex(): number {
    const end = this.currentPage * this.pageSize;
    return end > this.enregistrements.length ? this.enregistrements.length : end;
  }

  pagesArray(): number[] {
    return Array(this.totalPages).fill(0).map((_, i) => i + 1);
  }

  updatePagination(): void {
    this.totalPages = Math.ceil(this.enregistrements.length / this.pageSize);
    if (this.currentPage > this.totalPages) this.currentPage = 1;
  }
 getProcessusCode(e: EnregistrementDto): string {
  if (e.processusCode) return e.processusCode;  // ← priorité
  const found = this.processusList.find(p => p.id === e.processusId);
  return found ? found.code : e.processusId.slice(0, 8);
}
getFileName(fullPath: string): string {
  // Extraire le nom après le dernier slash
  const parts = fullPath.split('/');
  let fileName = parts[parts.length - 1];
  // Supprimer le préfixe UUID (tout ce qui précède le dernier underscore)
  const lastUnderscore = fileName.lastIndexOf('_');
  if (lastUnderscore !== -1) {
    fileName = fileName.substring(lastUnderscore + 1);
  }
  return fileName;
}
}
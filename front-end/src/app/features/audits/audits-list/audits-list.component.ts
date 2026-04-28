// src/app/features/audits/audits-list/audits-list.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { AuditService } from '../../../core/services/audit.service';
import { Audit, Evaluation, CreateAuditDto, CreateEvaluationDto, AuditType, AuditFrequence } from '../../../shared/models/audit.model';
import { AuthService } from '../../../core/services/auth.service';
interface AuditFormState {
  nom: string;
  description: string;
  processusId: string;
  type: AuditType;
  frequence: AuditFrequence;
  responsableId: string;
  actif: boolean;
}

interface EvalFormState {
  dateEvaluation: string;
  conforme: boolean;
  commentaire: string;
  evalueParId: string;
}

@Component({
  selector: 'app-audits-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audits-list.component.html',
  styleUrls: ['./audits-list.component.scss']
})
export class AuditsListComponent implements OnInit, OnDestroy {
  canEdit = false;
  isAuditeur = false;
  allAudits: Audit[] = [];
  filteredAudits: Audit[] = [];
  // Pagination
currentPage = 1;
pageSize = 7;
totalPages = 1;
  processus: { id: string; code: string; nom: string }[] = [];
  responsables: { id: number; nom: string }[] = [];
  users: { id: number; nom: string }[] = [];
  
  searchTerm = '';
  processusFilter = '';
  
  showFormModal = false;
  showEvalModal = false;
  showDeleteModal = false;
  
  selectedAudit: Audit | null = null;
  isEdit = false;
  
  form: AuditFormState = this.getEmptyForm();
  evalForm: EvalFormState = this.getEmptyEvalForm();
  formErrors: string[] = [];
  evalErrors: string[] = [];
  
  private subscriptions: Subscription[] = [];
  
  constructor(private auditService: AuditService,private authService: AuthService ) {}
  
  ngOnInit(): void {
    const role = this.authService.getRole();
    this.canEdit = role === 'ADMIN_ORG' || role === 'RESPONSABLE_SMQ';
      this.isAuditeur = role === 'AUDITEUR';
    this.loadData();
  }
  
  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
  
  private loadData(): void {
    // Charger les processus avec logs
    this.subscriptions.push(
      this.auditService.getProcessus().subscribe({
        next: (data) => {
          console.log('=== LISTE DES PROCESSUS CHARGÉE ===');
          console.log('Nombre de processus:', data.length);
          data.forEach(p => {
            console.log(`- ${p.code}: id=${p.id}, type=${typeof p.id}`);
          });
          this.processus = data;
        },
        error: (err) => console.error('Erreur chargement processus:', err)
      })
    );
    
    // Charger les responsables
    this.subscriptions.push(
      this.auditService.getResponsables().subscribe({
        next: (data) => this.responsables = data,
        error: (err) => console.error('Erreur chargement responsables:', err)
      })
    );
    
    // Charger les utilisateurs
    this.subscriptions.push(
      this.auditService.getUsers().subscribe({
        next: (data) => this.users = data,
        error: (err) => console.error('Erreur chargement utilisateurs:', err)
      })
    );
    
    // Charger les audits
    this.subscriptions.push(
      this.auditService.getAudits().subscribe({
        next: (data) => {
          this.allAudits = data;
          this.applyFilters();
        },
        error: (err) => console.error('Erreur chargement audits:', err)
      })
    );
  }
  
  applyFilters(): void {
    let list = [...this.allAudits];
    
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      list = list.filter(a => 
        a.nom.toLowerCase().includes(term) ||
        (a.description && a.description.toLowerCase().includes(term)) ||
        (a.responsableNom && a.responsableNom.toLowerCase().includes(term))
      );
    }
    
    if (this.processusFilter) {
      list = list.filter(a => a.processusId === this.processusFilter);
    }
    
    this.filteredAudits = list;
    this.currentPage = 1;
  this.updatePagination();
  }
  
  onSearchChange(): void {
    this.applyFilters();
  }
  
  onProcessusChange(): void {
    this.applyFilters();
  }
  
  openCreate(): void {
    this.isEdit = false;
    this.form = this.getEmptyForm();
    this.formErrors = [];
    this.showFormModal = true;
  }
  
  openEdit(audit: Audit): void {
    this.isEdit = true;
    this.selectedAudit = audit;
    this.form = {
      nom: audit.nom,
      description: audit.description || '',
      processusId: audit.processusId || '',
      type: audit.type,
      frequence: audit.frequence,
      responsableId: String(audit.responsableId || ''),
      actif: audit.actif
    };
    this.formErrors = [];
    this.showFormModal = true;
  }
  
  closeForm(): void {
    this.showFormModal = false;
    this.formErrors = [];
    this.selectedAudit = null;
  }
  
  saveAudit(): void {
    this.formErrors = [];
    
    if (!this.form.nom.trim()) {
      this.formErrors.push('Le nom est obligatoire.');
    }
    if (!this.form.processusId) {
      this.formErrors.push('Le processus est obligatoire.');
    }
    if (this.formErrors.length) return;
    
    // LOGS POUR DÉBOGUER
    console.log('=== SAVE AUDIT ===');
    console.log('processusId sélectionné:', this.form.processusId);
    console.log('Type du processusId:', typeof this.form.processusId);
    
    const selectedProcessus = this.processus.find(p => p.id === this.form.processusId);
    console.log('Processus trouvé:', selectedProcessus);
    
    const dto: CreateAuditDto = {
      nom: this.form.nom.trim(),
      description: this.form.description?.trim() || undefined,
      processusId: this.form.processusId,
      type: this.form.type,
      frequence: this.form.frequence,
      responsableId: this.form.responsableId ? Number(this.form.responsableId) : undefined,
      actif: this.form.actif === true
    };
    
    console.log('DTO envoyé au backend:', JSON.stringify(dto, null, 2));
    
    if (this.isEdit && this.selectedAudit) {
      this.subscriptions.push(
        this.auditService.updateAudit(this.selectedAudit.id, dto).subscribe({
          next: (updated) => {
            const index = this.allAudits.findIndex(a => a.id === updated.id);
            if (index !== -1) {
              this.allAudits[index] = updated;
              this.applyFilters();
            }
            this.closeForm();
          },
          error: (err) => {
            console.error('Erreur mise à jour:', err);
            this.formErrors.push('Erreur lors de la mise à jour');
          }
        })
      );
    } else {
      this.subscriptions.push(
        this.auditService.createAudit(dto).subscribe({
          next: (created) => {
            console.log('Création réussie, réponse:', created);
            this.allAudits.unshift(created);
            this.applyFilters();
            this.closeForm();
          },
          error: (err) => {
            console.error('Erreur création:', err);
            this.formErrors.push('Erreur lors de la création');
          }
        })
      );
    }
  }
  
  openEval(audit: Audit): void {
    this.selectedAudit = audit;
    this.evalForm = this.getEmptyEvalForm();
    this.evalErrors = [];
    this.showEvalModal = true;
  }
  
  closeEval(): void {
    this.showEvalModal = false;
    this.evalErrors = [];
    this.selectedAudit = null;
  }
  
saveEval(): void {
  this.evalErrors = [];
  
  if (!this.evalForm.dateEvaluation) {
    this.evalErrors.push('La date est obligatoire.');
  }
  if (!this.evalForm.evalueParId) {
    this.evalErrors.push('L\'évaluateur est obligatoire.');
  }
  if (this.evalErrors.length) return;
  
  // 🔧 CORRECTION 1: Convertir conforme en boolean (sans comparer avec string)
  const conformeValue = this.evalForm.conforme === true;
  
  // 🔧 CORRECTION 2: Convertir la date au format ISO complet
  const dateObj = new Date(this.evalForm.dateEvaluation);
  const formattedDate = dateObj.toISOString();
  
  const dto: CreateEvaluationDto = {
    dateEvaluation: formattedDate,
    conforme: conformeValue,
    commentaire: this.evalForm.commentaire?.trim() || undefined,
    evalueParId: Number(this.evalForm.evalueParId)
  };
  
  console.log('=== ADD EVALUATION ===');
  console.log('Audit ID:', this.selectedAudit!.id);
  console.log('DTO corrigé:', dto);
  
  this.subscriptions.push(
    this.auditService.addEvaluation(this.selectedAudit!.id, dto).subscribe({
      next: (evaluation) => {
        console.log('Évaluation ajoutée:', evaluation);
        const index = this.allAudits.findIndex(a => a.id === this.selectedAudit!.id);
        if (index !== -1) {
          if (!this.allAudits[index].evaluations) {
            this.allAudits[index].evaluations = [];
          }
          this.allAudits[index].evaluations!.unshift(evaluation);
          this.allAudits[index].derniereEvaluation = evaluation;
          this.applyFilters();
        }
        this.closeEval();
      },
      error: (err) => {
        console.error('Erreur complète:', err);
        if (err.error?.errors) {
          console.error('Erreurs de validation:', err.error.errors);
          // 🔧 CORRECTION 3: Convertir les messages en string
          const messages: string[] = [];
          for (const key in err.error.errors) {
            const value = err.error.errors[key];
            if (Array.isArray(value)) {
              messages.push(...value);
            } else if (typeof value === 'string') {
              messages.push(value);
            }
          }
          this.evalErrors.push(...messages);
        } else if (err.error?.message) {
          this.evalErrors.push(err.error.message);
        } else {
          this.evalErrors.push('Erreur lors de l\'ajout de l\'évaluation');
        }
      }
    })
  );
}
  openDelete(audit: Audit): void {
    this.selectedAudit = audit;
    this.showDeleteModal = true;
  }
  
  closeDelete(): void {
    this.showDeleteModal = false;
    this.selectedAudit = null;
  }
  
  confirmDelete(): void {
    if (!this.selectedAudit) return;
    
    this.subscriptions.push(
      this.auditService.deleteAudit(this.selectedAudit.id).subscribe({
        next: () => {
          this.allAudits = this.allAudits.filter(a => a.id !== this.selectedAudit!.id);
          this.applyFilters();
          this.closeDelete();
        },
        error: (err) => {
          console.error('Erreur suppression:', err);
          this.closeDelete();
        }
      })
    );
  }
  
  private getEmptyForm(): AuditFormState {
    return {
      nom: '',
      description: '',
      processusId: '',
      type: 'DOCUMENTAIRE',
      frequence: 'ANNUEL',
      responsableId: '',
      actif: true
    };
  }
  
  private getEmptyEvalForm(): EvalFormState {
    return {
      dateEvaluation: '',
      conforme: true,
      commentaire: '',
      evalueParId: ''
    };
  }
  
  getTypeClass(type: AuditType): string {
    const classes: Record<AuditType, string> = {
      'DOCUMENTAIRE': 'aud-type-doc',
      'OPERATIONNEL': 'aud-type-op',
      'REGLEMENTAIRE': 'aud-type-reg'
    };
    return classes[type] || '';
  }
  
  getEvalClass(evaluation?: Evaluation): string {
    if (!evaluation) return '';
    return evaluation.conforme ? 'aud-eval-ok' : 'aud-eval-ko';
  }
  
  getEvalLabel(evaluation?: Evaluation): string {
    if (!evaluation) return 'Aucune';
    return evaluation.conforme ? '✓ Conforme' : '✗ Non conforme';
  }
  
  getTypeLabel(type: AuditType): string {
    const labels: Record<AuditType, string> = {
      'DOCUMENTAIRE': 'Documentaire',
      'OPERATIONNEL': 'Opérationnel',
      'REGLEMENTAIRE': 'Réglementaire'
    };
    return labels[type] || type;
  }
  
  getFrequenceLabel(frequence: AuditFrequence): string {
    const labels: Record<AuditFrequence, string> = {
      'QUOTIDIEN': 'Quotidien',
      'HEBDOMADAIRE': 'Hebdomadaire',
      'MENSUEL': 'Mensuel',
      'TRIMESTRIEL': 'Trimestriel',
      'ANNUEL': 'Annuel'
    };
    return labels[frequence] || frequence;
  }
  get paginatedAudits(): Audit[] {
  const start = (this.currentPage - 1) * this.pageSize;
  return this.filteredAudits.slice(start, start + this.pageSize);
}

get lastIndex(): number {
  const end = this.currentPage * this.pageSize;
  return end > this.filteredAudits.length ? this.filteredAudits.length : end;
}

pagesArray(): number[] {
  return Array(this.totalPages).fill(0).map((_, i) => i + 1);
}

updatePagination(): void {
  this.totalPages = Math.ceil(this.filteredAudits.length / this.pageSize);
  if (this.currentPage > this.totalPages) this.currentPage = 1;
}
  
}
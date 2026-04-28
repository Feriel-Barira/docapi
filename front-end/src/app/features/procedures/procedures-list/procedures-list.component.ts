import { Component, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ProcedureService } from '../../../core/services/procedure.service';
import { ProcessService } from '../../../core/services/process.service';
import { Procedure, Instruction, DocumentLie } from '../../../shared/models/procedure.model';
import { environment } from '../../../../environments/environment';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { firstValueFrom } from 'rxjs';

interface FormState {
  code:               string;
  titre:              string;
  objectif:           string;
  domaineApplication: string;
  description:        string;
  processusId:        string;
  responsableId:      string;
  statut:             'ACTIF' | 'INACTIF';
  instructions:       Omit<Instruction, 'organisationId' | 'procedureId' | 'dateCreation'>[];
  selectedDocIds:     string[];
}

interface ProcessusRef { id: string; code: string; nom: string; }
interface ResponsableRef { id: string; nom: string; initiales: string; couleur: string; }

@Component({
  selector: 'app-procedures-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './procedures-list.component.html',
  styleUrls: ['./procedures-list.component.scss'],
  host: { style: 'display:flex;flex-direction:column;flex:1;overflow:hidden;min-height:0;' }
})
export class ProceduresListComponent implements OnInit, OnDestroy {
  submitted = false;
  canEdit = false;
  originalCode: string = '';
codeError = false;
  procedures: Procedure[]         = [];
  filteredProcedures: Procedure[] = [];

  searchTerm        = '';
  selectedProcessus = '';
  selectedProcessusLabel = '';
  currentPage       = 1;
  pageSize          = 10;

  showFormModal   = false;
  showDetailModal = false;
  showDeleteModal = false;

  selectedProcedure: Procedure | null = null;
  isEdit  = false;
  loading = false;
  form: FormState = this.emptyForm();

  // ── Référentiels dynamiques ───────────────────────────────────────────
  processusDisponibles:   ProcessusRef[]   = [];
  responsablesDisponibles: ResponsableRef[] = [];

  documentsDisponibles: DocumentLie[] = [];

  private nextId = 100;

  constructor(
    private procedureService: ProcedureService,
    private processService:   ProcessService,
    private http:             HttpClient,
    private renderer:         Renderer2,
     private notification:     NotificationService,
     private authService:      AuthService
  ) {
    const role = this.authService.getRole();
    this.canEdit = role === 'ADMIN_ORG' || role === 'RESPONSABLE_SMQ';
  }

  ngOnInit(): void {
    this.loadProcessus();
    this.loadResponsables();
    this.loadDocuments();
    this.loadProcedures();
  }

  ngOnDestroy(): void {
    this.renderer.removeClass(document.body, 'modal-open');
  }

  // ── Chargement référentiels ───────────────────────────────────────────
  private loadProcessus(): void {
    this.processService.getProcesses().subscribe({
      next: data => {
        
        this.processusDisponibles = data.map(p => ({
          id:   p.id,
          code: p.code,
          nom:  p.nom
        }));
      },
      error: () => {}
    });
  }

  private loadResponsables(): void {
      const orgId = localStorage.getItem('auth_organisation_id') || '';
    const token = localStorage.getItem('auth_token') || '';
    this.http.get<any[]>(`${environment.apiUrl}/Users/responsables?organisationId=${orgId}`, {
      headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` })
    }).subscribe({
      next: users => {
        console.log('Users pour responsables:', users);
        const colors = ['#166534','#6b21a8','#7c2d12','#155e75','#92400e','#1e3a5f'];
        this.responsablesDisponibles = users.map((u, i) => ({
          id:        String(u.Id || u.id),
          nom:       `${u.Prenom || u.prenom || ''} ${u.Nom || u.nom || u.Username || u.username || ''}`.trim(),
          initiales: this.buildInitiales(u),
          couleur:   colors[i % colors.length]
        }));
      },
      error: (err) => {
  console.error('Erreur chargement responsables:', err);
}
    });
  }
  private loadDocuments(): void {
  const orgId = localStorage.getItem('auth_organisation_id') ?? '';
  const token = localStorage.getItem('auth_token') || '';
  this.http.get<any[]>(`${environment.apiUrl}/Document?organisationId=${orgId}`, {
    headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` })
  }).subscribe({
    next: docs => {
      this.documentsDisponibles = docs.map(d => ({
        id:           d.Id    || d.id,
        code:         d.Code  || d.code,
        titre:        d.Titre || d.titre,
        typeDocument: d.TypeDocument || d.typeDocument || 'REFERENCE'
      }));
    },
    error: () => {}
  });
}

  private buildInitiales(u: any): string {
    const prenom = u.Prenom || u.prenom || '';
    const nom    = u.Nom    || u.nom    || u.Username || u.username || '';
    if (prenom && nom) return `${prenom[0]}${nom[0]}`.toUpperCase();
    if (nom)           return nom.substring(0, 2).toUpperCase();
    return '?';
  }

  // ── Chargement procédures ─────────────────────────────────────────────
  loadProcedures(): void {
    this.loading = true;
    this.procedureService.getProcedures().subscribe({
      next: data => {
          console.log('RAW data[0]:', JSON.stringify(data[0]));

        console.log('Procédure exemple:', data[0]);
      console.log('ProcessusId:', data[0].processusId);
      console.log('ResponsableId:', data[0].responsableId);
      console.log('Processus dispo:', this.processusDisponibles[0]);
        this.procedures = data;
        this.applyFilters();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  // ── Filtres ───────────────────────────────────────────────────────────
  applyFilters(): void {
    let filtered = [...this.procedures];
    if (this.searchTerm) {
      const t = this.searchTerm.toLowerCase();
      filtered = filtered.filter(p =>
        p.titre.toLowerCase().includes(t) ||
        p.code.toLowerCase().includes(t)  ||
        this.getResponsableNom(p).toLowerCase().includes(t)
      );
    }
    if (this.selectedProcessus) {
      filtered = filtered.filter(p => p.processusId === this.selectedProcessus);
    }
    this.filteredProcedures = filtered;
    this.currentPage = 1;
  }

  onSearchChange(v: string): void    { this.searchTerm = v; this.applyFilters(); }
  onProcessusChange(v: string): void {
    this.selectedProcessus = v;
    const proc = this.processusDisponibles.find(p => p.id === v);
    this.selectedProcessusLabel = proc ? `${proc.code} — ${proc.nom}` : '';
    this.applyFilters();
  }

  // ── Pagination ────────────────────────────────────────────────────────
  get paginatedProcedures(): Procedure[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredProcedures.slice(start, start + this.pageSize);
  }
  get totalPages(): number { return Math.ceil(this.filteredProcedures.length / this.pageSize); }
  get lastIndex(): number  { return Math.min(this.currentPage * this.pageSize, this.filteredProcedures.length); }
  pagesArray(): number[]   { return Array.from({ length: this.totalPages }, (_, i) => i + 1); }

  // ── Helpers affichage ─────────────────────────────────────────────────
  getResponsableNom(p: Procedure): string {
    if (p.responsable?.prenom && p.responsable?.nom)
      return `${p.responsable.prenom} ${p.responsable.nom}`;
    const r = this.responsablesDisponibles.find(r => String(r.id) === String(p.responsableId));
    return r?.nom ?? '—';
  }

  getResponsableInitiales(p: Procedure): string {
  const r = this.responsablesDisponibles.find(r => String(r.id) === String(p.responsableId));
  return r?.initiales ?? '?';
}

  getResponsableCouleur(p: Procedure): string {
    const r = this.responsablesDisponibles.find(r => String(r.id) === String(p.responsableId));
    return r?.couleur ?? '#374151';
  }

getProcessusCode(p: Procedure): string {
  if (p.processusCode) return p.processusCode;
  if (p.processus?.code) return p.processus.code;
  const proc = this.processusDisponibles.find(x => x.id === p.processusId);
  return proc?.code ?? '—';
}

  getProcessusNom(p: Procedure): string {
    if (p.processus) return p.processus.nom;
    const proc = this.processusDisponibles.find(x => x.id === p.processusId);
    return proc?.nom ?? '—';
  }

  // ── Modals ────────────────────────────────────────────────────────────
  private openModalState():  void { this.renderer.addClass(document.body, 'modal-open'); }
  private closeModalState(): void { this.renderer.removeClass(document.body, 'modal-open'); }

  openDetail(p: Procedure): void {
    this.selectedProcedure = p;
    this.showDetailModal   = true;
    this.openModalState();
  }
  closeDetail(): void {
    this.showDetailModal   = false;
    this.selectedProcedure = null;
    this.closeModalState();
  }
  openEditFromDetail(): void {
    const p = this.selectedProcedure;
    this.closeDetail();
    if (p) this.openEdit(p);
  }

  openCreate(): void {
    this.isEdit        = false;
    this.form          = this.emptyForm();
    this.showFormModal = true;
    this.openModalState();
     this.submitted = false;   // ← ajouter
  this.codeError = false; 
  }

  openEdit(p: Procedure): void {
    this.originalCode = p.code;
    this.isEdit            = true;
    this.selectedProcedure = p;
    this.form = {
      code:               p.code,
      titre:              p.titre,
      objectif:           p.objectif,
      domaineApplication: p.domaineApplication,
      description:        p.description,
      processusId:        p.processusId,
      responsableId:      String(p.responsableId),
      statut:             p.statut,
      instructions:       (p.instructions ?? []).map(i => ({
        id: i.id, code: i.code, titre: i.titre,
        description: i.description, statut: i.statut
      })),
      selectedDocIds: (p.documentsAssocies ?? []).map(d => d.id)
    };
      this.submitted = false;   // ← ajouter
  this.codeError = false;
    this.showFormModal = true;
    this.openModalState();
  }

  closeForm(): void { this.showFormModal = false; this.closeModalState(); }

  // ── Instructions ──────────────────────────────────────────────────────
  addInstruction(): void {
    const n = this.form.instructions.length + 1;
    this.form.instructions.push({
      id: 'new-' + Date.now(),
      code: `INS-00${n}`, titre: '', description: '', statut: 'ACTIF'
    });
  }
  removeInstruction(i: number): void { this.form.instructions.splice(i, 1); }

  // ── Documents ─────────────────────────────────────────────────────────
  isDocSelected(docId: string): boolean { return this.form.selectedDocIds.includes(docId); }
  toggleDoc(docId: string): void {
    const idx = this.form.selectedDocIds.indexOf(docId);
    if (idx === -1) this.form.selectedDocIds.push(docId);
    else            this.form.selectedDocIds.splice(idx, 1);
  }

  // ── Save ──────────────────────────────────────────────────────────────
  async saveProcedure():Promise<void> {
    this.submitted = true;
    // ✅ Vérification d'unicité (sans rien casser)
  if (this.form.code !== this.originalCode) {
    const exists = await firstValueFrom(
      this.procedureService.checkProcedureCode(this.form.code, this.selectedProcedure?.id)
    );
    if (exists) {
      this.codeError = true;
      return;  // ← on bloque l'envoi, pas de toast
    }
  }
  if (!this.form.code || !this.form.titre || !this.form.processusId || !this.form.responsableId) return;

  const dto = {
  code:               this.form.code,
  titre:              this.form.titre,
  objectif:           this.form.objectif,
  domaineApplication: this.form.domaineApplication,
  description:        this.form.description,
  responsableId:      this.form.responsableId,
  statut:             this.form.statut,
  instructions:       this.form.instructions.map((ins, idx) => ({  // ← AJOUTER
    code:        ins.code,
    titre:       ins.titre,
    description: ins.description,
    ordre:       idx + 1,
    statut:      ins.statut
  }))
};

  if (this.isEdit && this.selectedProcedure) {
    this.procedureService.updateProcedure(this.selectedProcedure.id, dto).subscribe({
      next: () => {
        this.notification.showSuccess('Procédure modifiée avec succès !');
        this.loadProcedures();
        this.closeForm();
      },
      error: () => this.notification.showError('Erreur lors de la modification.')
    });
  } else {
    this.procedureService.createProcedure({
      ...dto,
      processusId: this.form.processusId
    } as any).subscribe({
      next: () => {
        this.notification.showSuccess('Procédure créée avec succès !');
        this.loadProcedures();
        this.closeForm();
      },
      error: () => this.notification.showError('Erreur lors de la création.')
    });
  }
}

  // ── Suppression ───────────────────────────────────────────────────────
  openDelete(p: Procedure): void {
    this.selectedProcedure = p;
    this.showDeleteModal   = true;
    this.openModalState();
  }
  closeDelete(): void {
    this.showDeleteModal   = false;
    this.selectedProcedure = null;
    this.closeModalState();
  }
  confirmDelete(): void {
  if (!this.selectedProcedure) return;
  this.procedureService.deleteProcedure(this.selectedProcedure.id).subscribe({
    next: () => {
      this.notification.showSuccess('Procédure supprimée avec succès !');
      this.loadProcedures();
      this.closeDelete();
    },
    error: () => this.notification.showError('Erreur lors de la suppression.')
  });
}

  private emptyForm(): FormState {
    return {
      code: '', titre: '', objectif: '', domaineApplication: '',
      description: '', processusId: '', responsableId: '',
      statut: 'ACTIF', instructions: [], selectedDocIds: []
    };
  }
  onCodeChange(): void {
  this.codeError = false;
  this.submitted = false; 
}
}
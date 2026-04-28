// src/app/features/non-conformites/non-conformites-list/non-conformites.component.ts
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NonConformiteService } from '../../../core/services/non-conformite.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  NonConformite,
  ActionCorrective,
  CreateNonConformiteDto,
  NCGravite,
  NCStatut,
  NCType,
  NCNature,
  NCSource,
  ACType,
} from '../../../shared/models/non-conformite.model';
import { EnregistrementService } from 'src/app/core/services/enregistrement.service';

interface FormState {
  description: string;
  type: NCType;
  nature: NCNature;
  source: NCSource;
  gravite: NCGravite;
  processusId: string;
  responsableId: string;
  detecteParId: string;
  dateDetection: string;
  acDescription: string;
  acType: ACType;
  acResponsableId: string;
  acEcheance: string;
}

interface ACFormState {
  description: string;
  type: ACType;
  responsableId: string;
  echeance: string;
}

interface AnalyseFormState {
  methodeAnalyse: string;
  description: string;
}

@Component({
  selector: 'app-non-conformites-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: '../non-conformites-list/non-conformites.component.html',
  styleUrls: ['../non-conformites-list/non-conformites.component.scss']
})
export class NonConformitesListComponent implements OnInit {
  canEdit = false;

  // ── DATA ──────────────────────────────────────────────────
  allNC: NonConformite[] = [];
  filteredNC: NonConformite[] = [];
  responsables: { id: number; nom: string }[] = [];
  processus: { id: string; code: string; nom: string }[] = [];
  loading = false;

  showPreuvePicker = false;
  actionEnCours: ActionCorrective | null = null;
  preuvesDisponibles: any[] = [];
  uploadPreuveFile: File | null = null;
  uploadPreuveDescription = '';
  uploadPreuveProcessusId = '';

  // Pagination
  currentPage = 1;
  pageSize = 7;
  totalPages = 1;

  // ── FILTERS ───────────────────────────────────────────────
  searchTerm = '';
  statutFilter: NCStatut | '' = '';

  // ── MODALS ────────────────────────────────────────────────
  showDetailModal = false;
  showFormModal = false;
  showDeleteModal = false;
  showACForm = false;
  showAnalyseModal = false;

  selectedNC: NonConformite | null = null;
  isEdit = false;
  selectedPreuveId: string = '';

  // ── FORMS ─────────────────────────────────────────────────
  form: FormState = this.emptyForm();
  acForm: ACFormState = this.emptyACForm();
  analyseForm: AnalyseFormState = this.emptyAnalyseForm();
  formErrors: string[] = [];
  analyseErrors: string[] = [];

  constructor(
    private ncService: NonConformiteService,
    private cdr: ChangeDetectorRef,
    private enregService: EnregistrementService,
    private notification: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const role = this.authService.getRole();
    this.canEdit = role === 'ADMIN_ORG' || role === 'RESPONSABLE_SMQ';
    this.loadData();
  }

  loadData(): void {
    this.loading = true;

    this.ncService.getResponsablesFromApi().subscribe(data => {
      this.responsables = data;
    });

    this.ncService.getProcessusFromApi().subscribe(data => {
      this.processus = data;
    });

    this.ncService.getNonConformites().subscribe({
      next: data => {
        this.allNC = data;
        this.applyFilters();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  // ── FILTERS ───────────────────────────────────────────────
  applyFilters(): void {
    let list = [...this.allNC];
    if (this.searchTerm.trim()) {
      const q = this.searchTerm.toLowerCase();
      list = list.filter(nc =>
        nc.reference.toLowerCase().includes(q) ||
        nc.description.toLowerCase().includes(q) ||
        (nc.responsableNom || '').toLowerCase().includes(q)
      );
    }
    if (this.statutFilter) {
      list = list.filter(nc => nc.statut === this.statutFilter);
    }
    this.filteredNC = list;
    this.currentPage = 1;
    this.updatePagination();
  }

  onSearchChange(): void { this.applyFilters(); }
  setStatutFilter(s: NCStatut | ''): void { this.statutFilter = s; this.applyFilters(); }

  // ── STATS ─────────────────────────────────────────────────
  get totalNC(): number { return this.allNC.length; }
  get ouvertesNC(): number { return this.allNC.filter(nc => nc.statut === 'OUVERTE').length; }
  get enCoursNC(): number { return this.allNC.filter(nc => nc.statut === 'ACTION_EN_COURS' || nc.statut === 'ANALYSE').length; }
  get clotureesNC(): number { return this.allNC.filter(nc => nc.statut === 'CLOTUREE').length; }

  // ── DETAIL ────────────────────────────────────────────────
  openDetail(nc: NonConformite): void {
    this.ncService.getNonConformiteById(nc.id).subscribe(completeNc => {
      this.selectedNC = completeNc;
      this.showACForm = false;
      this.acForm = this.emptyACForm();
      this.showDetailModal = true;
      this.loadHistorique();
    });
  }

  closeDetail(): void {
    this.showDetailModal = false;
    this.selectedNC = null;
    this.showACForm = false;
  }

  loadHistorique(): void {
    if (!this.selectedNC) return;
    this.ncService.getHistorique(this.selectedNC.id).subscribe({
      next: (data) => {
        if (this.selectedNC && data) {
          this.selectedNC.historique = data.sort((a, b) =>
            new Date(a.dateChangement).getTime() - new Date(b.dateChangement).getTime()
          );
        }
      },
      error: (err) => console.error('Erreur chargement historique:', err)
    });
  }

  // ── MODAL FORM (CREATE / EDIT) ────────────────────────────
  openCreate(): void {
    if (!this.canEdit) return;
    this.isEdit = false;
    this.form = this.emptyForm();
    this.formErrors = [];
    this.showFormModal = true;
  }

  openEdit(nc: NonConformite): void {
    if (!this.canEdit) return;
    this.isEdit = true;
    this.selectedNC = nc;
    this.form = {
      description: nc.description,
      type: nc.type,
      nature: nc.nature,
      source: nc.source,
      gravite: nc.gravite,
      processusId: nc.processusId || '',
      responsableId: String(nc.responsableId || ''),
      detecteParId: String(nc.detecteParId || ''),
      dateDetection: nc.dateDetection,
      acDescription: '',
      acType: 'CURATIVE',
      acResponsableId: '',
      acEcheance: ''
    };
    this.formErrors = [];
    this.showFormModal = true;
  }

  closeForm(): void {
    this.showFormModal = false;
    this.formErrors = [];
  }

  saveNC(): void {
    this.formErrors = [];
    if (!this.form.description.trim()) this.formErrors.push('La description est obligatoire.');
    if (!this.form.responsableId) this.formErrors.push('Le responsable est obligatoire.');
    if (!this.form.detecteParId) this.formErrors.push('Détecté par est obligatoire.');
    if (!this.form.dateDetection) this.formErrors.push('La date de détection est obligatoire.');
    if (this.formErrors.length) return;

    if (this.isEdit && this.selectedNC) {
      const updated: any = {
        description: this.form.description,
        type: this.form.type,
        nature: this.form.nature,
        source: this.form.source,
        gravite: this.form.gravite,
        processusId: this.form.processusId || undefined,
        processusCode: this.processus.find(p => p.id === this.form.processusId)?.code,
        responsableTraitementId: Number(this.form.responsableId),
        responsableNom: this.responsables.find(r => r.id === Number(this.form.responsableId))?.nom,
        detecteParId: Number(this.form.detecteParId),
        detecteParNom: this.responsables.find(r => r.id === Number(this.form.detecteParId))?.nom,
        dateDetection: this.form.dateDetection
      };
      this.ncService.updateNonConformite(this.selectedNC.id, updated).subscribe(() => {
        const idx = this.allNC.findIndex(n => n.id === this.selectedNC!.id);
        if (idx >= 0) this.allNC[idx] = { ...this.allNC[idx], ...updated };
        this.applyFilters();
        this.closeForm();
      });
    } else {
      const dto: CreateNonConformiteDto = {
        description: this.form.description,
        type: this.form.type,
        nature: this.form.nature,
        source: this.form.source,
        gravite: this.form.gravite,
        processusId: this.form.processusId || undefined,
        responsableTraitementId: Number(this.form.responsableId),
        detecteParId: Number(this.form.detecteParId),
        dateDetection: this.form.dateDetection,
        actionInitiale: this.form.acDescription.trim() ? {
          description: this.form.acDescription,
          type: this.form.acType,
          responsableId: this.form.acResponsableId ? Number(this.form.acResponsableId) : undefined,
          echeance: this.form.acEcheance || undefined
        } : undefined
      };

      this.ncService.createNonConformite(dto).subscribe(nc => {
        if (this.form.acDescription && this.form.acDescription.trim()) {
          let formattedEcheance: string | undefined = undefined;
          if (this.form.acEcheance) {
            const dateObj = new Date(this.form.acEcheance);
            dateObj.setHours(12, 0, 0);
            formattedEcheance = dateObj.toISOString();
          }

          const acPayload = {
            description: this.form.acDescription,
            type: this.form.acType,
            responsableId: this.form.acResponsableId ? Number(this.form.acResponsableId) : undefined,
            dateEcheance: formattedEcheance
          };

          this.ncService.addActionCorrective(nc.id, acPayload as any).subscribe(() => {
            this.ncService.getNonConformiteById(nc.id).subscribe(updatedNc => {
              this.allNC.unshift(updatedNc);
              this.applyFilters();
              this.closeForm();
            });
          });
        } else {
          this.ncService.getNonConformiteById(nc.id).subscribe(updatedNc => {
            this.allNC.unshift(updatedNc);
            this.applyFilters();
            this.closeForm();
          });
        }
      });
    }
  }

  // ── MODAL DELETE ──────────────────────────────────────────
  openDelete(nc: NonConformite): void {
    if (!this.canEdit) return;
    this.selectedNC = nc;
    this.showDeleteModal = true;
  }

  closeDelete(): void {
    this.showDeleteModal = false;
    this.selectedNC = null;
  }

  confirmDelete(): void {
    if (!this.canEdit) return;
    if (!this.selectedNC) return;
    this.ncService.deleteNonConformite(this.selectedNC.id).subscribe(() => {
      this.allNC = this.allNC.filter(n => n.id !== this.selectedNC!.id);
      this.applyFilters();
      this.closeDelete();
    });
  }

  // ── ACTION CORRECTIVE ─────────────────────────────────────
  toggleACForm(): void {
    this.showACForm = !this.showACForm;
    if (!this.showACForm) this.acForm = this.emptyACForm();
  }

  addAC(): void {
    if (!this.canEdit) return;
    if (!this.selectedNC || !this.acForm.description.trim()) return;

    let formattedEcheance: string | undefined = undefined;
    if (this.acForm.echeance) {
      const dateObj = new Date(this.acForm.echeance);
      dateObj.setHours(12, 0, 0);
      formattedEcheance = dateObj.toISOString();
    }

    const payload = {
      description: this.acForm.description,
      type: this.acForm.type,
      responsableId: this.acForm.responsableId ? Number(this.acForm.responsableId) : undefined,
      dateEcheance: formattedEcheance
    };

    this.ncService.addActionCorrective(this.selectedNC.id, payload as any).subscribe(ac => {
      ac.responsableNom = this.responsables.find(r => r.id === ac.responsableId)?.nom;
      if (!this.selectedNC!.actionsCorrectives) this.selectedNC!.actionsCorrectives = [];
      this.selectedNC!.actionsCorrectives!.push(ac);
      this.recalcAvancements();
      this.acForm = this.emptyACForm();
      this.showACForm = false;

      this.ncService.getNonConformiteById(this.selectedNC!.id).subscribe(updatedNc => {
        const idx = this.allNC.findIndex(n => n.id === updatedNc.id);
        if (idx >= 0) this.allNC[idx] = updatedNc;
        this.selectedNC = updatedNc;
        this.applyFilters();
        this.loadHistorique();
      });
    });
  }

  deleteAC(ac: ActionCorrective): void {
    if (!this.canEdit) return;
    if (!this.selectedNC) return;
    this.ncService.deleteAC(this.selectedNC.id, ac.id).subscribe(() => {
      this.selectedNC!.actionsCorrectives = this.selectedNC!.actionsCorrectives!.filter(a => a.id !== ac.id);
      this.recalcAvancements();

      this.ncService.getNonConformiteById(this.selectedNC!.id).subscribe(updatedNc => {
        const idx = this.allNC.findIndex(n => n.id === updatedNc.id);
        if (idx >= 0) this.allNC[idx] = updatedNc;
        this.selectedNC = updatedNc;
        this.applyFilters();
        this.loadHistorique();
      });
    });
  }

  updateACStatutForce(ac: ActionCorrective, newStatut: string): void {
    if (!this.canEdit) return;
    if (!this.selectedNC) return;

    const oldStatut = ac.statut;
    ac.statut = newStatut as any;

    this.ncService.updateACStatut(this.selectedNC.id, ac.id, newStatut).subscribe({
      next: () => {
        this.recalcAvancements();
        const ncId = this.selectedNC!.id;
        this.ncService.getNonConformiteById(ncId).subscribe(updatedNc => {
          this.selectedNC = updatedNc;
          const idx = this.allNC.findIndex(n => n.id === ncId);
          if (idx >= 0) this.allNC[idx] = updatedNc;
          this.applyFilters();
          this.loadHistorique();
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        console.error('Erreur:', err);
        ac.statut = oldStatut;
      }
    });
  }

  private recalcAvancements(): void {
    if (!this.selectedNC) return;
    const acs = this.selectedNC.actionsCorrectives || [];
    const total = acs.length;
    const done = acs.filter(a => a.statut === 'REALISEE' || a.statut === 'VERIFIEE').length;
    this.selectedNC.avancementAC = total > 0 ? Math.round((done / total) * 100) : 0;

    const idx = this.allNC.findIndex(n => n.id === this.selectedNC!.id);
    if (idx >= 0) {
      this.allNC[idx].avancementAC = this.selectedNC.avancementAC;
      this.allNC[idx].actionsCorrectives = [...acs];
    }
    this.applyFilters();
  }

  // ── ANALYSE DES CAUSES ────────────────────────────────────
  openAnalyseModal(): void {
    if (!this.canEdit) return;
    if (!this.selectedNC) return;
    this.analyseForm = {
      methodeAnalyse: this.selectedNC.analyseCause?.methodeAnalyse || 'CINQ_M',
      description: this.selectedNC.analyseCause?.description || ''
    };
    this.analyseErrors = [];
    this.showAnalyseModal = true;
  }

  editAnalyse(): void {
    this.openAnalyseModal();
  }

  closeAnalyseModal(): void {
    this.showAnalyseModal = false;
    this.analyseErrors = [];
  }

  saveAnalyse(): void {
    if (!this.canEdit) return;
    this.analyseErrors = [];

    if (!this.analyseForm.methodeAnalyse) {
      this.analyseErrors.push('La méthode d\'analyse est obligatoire.');
    }
    if (!this.analyseForm.description.trim()) {
      this.analyseErrors.push('La description de l\'analyse est obligatoire.');
    }
    if (this.analyseErrors.length) return;

    const dto = {
      methodeAnalyse: this.analyseForm.methodeAnalyse,
      description: this.analyseForm.description
    };

    if (this.selectedNC) {
      this.ncService.addAnalyse(this.selectedNC.id, dto).subscribe({
        next: () => {
          this.closeAnalyseModal();
          // Recharger depuis l'API pour avoir les données à jour
          this.ncService.getNonConformiteById(this.selectedNC!.id).subscribe(updatedNc => {
            this.selectedNC = updatedNc;
            const idx = this.allNC.findIndex(n => n.id === updatedNc.id);
            if (idx >= 0) this.allNC[idx] = updatedNc;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Erreur sauvegarde analyse:', err);
          this.notification.showError('Erreur lors de la sauvegarde de l\'analyse.');
        }
      });
    }
  }

  private refreshCurrentNC(): void {
    if (this.selectedNC) {
      this.ncService.getNonConformiteById(this.selectedNC.id).subscribe(updatedNc => {
        this.selectedNC = updatedNc;
        const idx = this.allNC.findIndex(n => n.id === updatedNc.id);
        if (idx >= 0) this.allNC[idx] = updatedNc;
        this.applyFilters();
        this.cdr.detectChanges();
      });
    }
  }

  changeStatut(nc: NonConformite, statut: NCStatut) {
    if (!this.canEdit) return;
    this.ncService.updateStatut(nc.id, statut).subscribe({
      next: () => {
        nc.statut = statut;
        const index = this.allNC.findIndex(n => n.id === nc.id);
        if (index !== -1) {
          this.allNC[index].statut = statut;
        }
        this.applyFilters();
        this.loadHistorique();
      },
      error: () => {
        alert('Erreur changement statut');
      }
    });
  }

  // ── HELPERS ───────────────────────────────────────────────
  private emptyForm(): FormState {
    return {
      description: '',
      type: 'PRODUIT_SERVICE',
      nature: 'REELLE',
      source: 'AUDIT',
      gravite: 'MINEURE',
      processusId: '',
      responsableId: '',
      detecteParId: '',
      dateDetection: new Date().toISOString().split('T')[0],
      acDescription: '',
      acType: 'CURATIVE',
      acResponsableId: '',
      acEcheance: ''
    };
  }

  private emptyACForm(): ACFormState {
    return { description: '', type: 'CURATIVE', responsableId: '', echeance: '' };
  }

  private emptyAnalyseForm(): AnalyseFormState {
    return { methodeAnalyse: 'CINQ_M', description: '' };
  }

  getGraviteClass(g: NCGravite): string {
    return { MINEURE: 'nc-mineure', MAJEURE: 'nc-majeure', CRITIQUE: 'nc-critique' }[g] || '';
  }

  getStatutClass(s: NCStatut): string {
    return {
      OUVERTE: 'nc-ouverte',
      ANALYSE: 'nc-analyse',
      ACTION_EN_COURS: 'nc-encours',
      CLOTUREE: 'nc-cloturee'
    }[s] || '';
  }

  getStatutLabel(s: NCStatut): string {
    return {
      OUVERTE: 'OUVERTE',
      ANALYSE: 'ANALYSE',
      ACTION_EN_COURS: 'ACTION_EN_COURS',
      CLOTUREE: 'CLOTUREE'
    }[s] || s;
  }

  getTypeLabel(t: NCType): string {
    return {
      PRODUIT_SERVICE: 'Produit / Service',
      PROCESSUS: 'Processus',
      SYSTEME: 'Système',
      ENVIRONNEMENT: 'Environnement'
    }[t] || t;
  }

  getNatureLabel(n: NCNature): string {
    return n === 'REELLE' ? 'Réelle' : 'Potentielle';
  }

  getSourceLabel(s: NCSource): string {
    return {
      AUDIT: 'Audit', RECLAMATION: 'Réclamation',
      POINT_CONTROLE: 'Point contrôle', INSPECTION: 'Inspection', OBSERVATION: 'Observation'
    }[s] || s;
  }

  getACTypeLabel(t: ACType): string {
    return { CURATIVE: 'Curative', CORRECTIVE: 'Corrective', PREVENTIVE: 'Préventive' }[t] || t;
  }

  getACStatutClass(statut: string): string {
    const classes: Record<string, string> = {
      'PLANIFIEE': 'ac-attente',
      'EN_COURS': 'ac-encours',
      'REALISEE': 'ac-terminee',
      'VERIFIEE': 'ac-terminee'
    };
    return classes[statut] || '';
  }

  getGraviteEmoji(g: NCGravite): string {
    return { MINEURE: '🟡', MAJEURE: '🟠', CRITIQUE: '🔴' }[g] || '';
  }

  getNatureTypeLine(nc: NonConformite): string {
    return `${nc.nature} — ${nc.type}`;
  }

 getACStatutOptions(currentStatut?: string): { value: string; label: string }[] {
  const allOptions = [
    { value: 'PLANIFIEE', label: 'Planifiée', order: 0 },
    { value: 'EN_COURS', label: 'En cours', order: 1 },
    { value: 'REALISEE', label: 'Réalisée', order: 2 },
    { value: 'VERIFIEE', label: 'Vérifiée', order: 3 }
  ];
  
  // Trouver l'ordre du statut actuel
  const currentOption = allOptions.find(opt => opt.value === currentStatut);
  const currentOrder = currentOption ? currentOption.order : 0;
  
  // Ne retourner que les options >= au statut actuel
  return allOptions
    .filter(opt => opt.order >= currentOrder)
    .map(opt => ({ value: opt.value, label: opt.label }));
}

  getMethodeAnalyseLabel(methode: string): string {
    const labels: Record<string, string> = {
      'CINQ_M': '5M (Matière, Matériel, Main-d\'œuvre, Méthode, Milieu)',
      'CINQM': '5M (Matière, Matériel, Main-d\'œuvre, Méthode, Milieu)',
      'ISHIKAWA': 'Diagramme d\'Ishikawa (Cause-effet)',
      'CINQ_POURQUOI': '5 Pourquoi',
      'CINQPOURQUOI': '5 Pourquoi',
      'AUTRE': 'Autre méthode'
    };
    return labels[methode] || methode;
  }

  // ── GESTION DES PREUVES POUR ACTIONS CORRECTIVES ──────────
  ouvrirPicker(ac: ActionCorrective) {
    this.actionEnCours = ac;
    const processusId = this.selectedNC?.processusId;
    if (processusId) {
      this.enregService.getAll(processusId).subscribe(preuves => {
        this.preuvesDisponibles = preuves;
        this.showPreuvePicker = true;
      });
    } else {
      this.preuvesDisponibles = [];
      this.showPreuvePicker = true;
    }
  }

  onPreuveChoisie(ac: ActionCorrective, enregistrementId: string) {
    this.ncService.attacherPreuve(ac.id, enregistrementId).subscribe(() => {
      ac.preuveEnregistrementId = enregistrementId;
    });
  }

  downloadPreuve(enregId: string): void {
    this.enregService.download(enregId).subscribe((blob: Blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'preuve';
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  detacherPreuve(ac: ActionCorrective) {
    this.ncService.detacherPreuve(ac.id).subscribe(() => {
      ac.preuveEnregistrementId = undefined;
    });
  }

  fermerPreuvePicker() {
    this.showPreuvePicker = false;
    this.actionEnCours = null;
    this.uploadPreuveFile = null;
    this.uploadPreuveDescription = '';
    this.selectedPreuveId = '';
  }

  onPreuveFileSelected(event: any) {
    this.uploadPreuveFile = event.target.files[0];
  }

  uploadNouvellePreuve() {
    if (!this.uploadPreuveFile || !this.selectedNC?.processusId) return;
    const formData = new FormData();
    const orgId = localStorage.getItem('auth_organisation_id') || '';
    formData.append('organisationId', orgId);
    formData.append('fichier', this.uploadPreuveFile);
    formData.append('processusId', this.selectedNC.processusId);
    formData.append('description', this.uploadPreuveDescription);
    formData.append('typeEnregistrement', 'PREUVE_EXECUTION');
    formData.append('reference', new Date().toISOString().slice(0, 19));
    this.enregService.upload(formData).subscribe({
      next: (res) => {
        this.ncService.attacherPreuve(this.actionEnCours!.id, res.id).subscribe(() => {
          this.actionEnCours!.preuveEnregistrementId = res.id;
          this.fermerPreuvePicker();
          this.refreshCurrentNC();
        });
      },
      error: () => alert('Erreur upload')
    });
  }

  selectionnerPreuveExistante(enregId: string) {
    if (!enregId) return;
    this.ncService.attacherPreuve(this.actionEnCours!.id, enregId).subscribe(() => {
      this.actionEnCours!.preuveEnregistrementId = enregId;
      this.fermerPreuvePicker();
      this.refreshCurrentNC();
    });
  }

  getFileName(fullPath: string): string {
    const parts = fullPath.split('/');
    let fileName = parts[parts.length - 1];
    const lastUnderscore = fileName.lastIndexOf('_');
    if (lastUnderscore !== -1) {
      fileName = fileName.substring(lastUnderscore + 1);
    }
    return fileName;
  }

  enregistrerPreuve() {
    if (this.selectedPreuveId) {
      this.selectionnerPreuveExistante(this.selectedPreuveId);
      return;
    }
    if (this.uploadPreuveFile) {
      this.uploadNouvellePreuve();
      return;
    }
    this.notification.showError('Veuillez sélectionner une preuve existante ou choisir un fichier à uploader.');
  }

  // ── PAGINATION ────────────────────────────────────────────
  get paginatedNC(): NonConformite[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredNC.slice(start, start + this.pageSize);
  }

  get lastIndex(): number {
    const end = this.currentPage * this.pageSize;
    return end > this.filteredNC.length ? this.filteredNC.length : end;
  }

  pagesArray(): number[] {
    return Array(this.totalPages).fill(0).map((_, i) => i + 1);
  }

  updatePagination(): void {
    this.totalPages = Math.ceil(this.filteredNC.length / this.pageSize);
    if (this.currentPage > this.totalPages) this.currentPage = 1;
  }
}
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IndicateurService } from '../../../core/services/indicateur.service';
import { Indicateur, CreateIndicateurDto, CreateValeurDto, IndFrequence, IndStatut } from '../../../shared/models/indicateur.model';
import { AuthService } from '../../../core/services/auth.service';

interface IndFormState {
  code: string;
  nom: string;
  description: string;
  processusId: string;
  responsableId: string;
  unite: string;
  methodeCalcul: string;
  valeurCible: string;
  seuilAlerte: string;
  frequence: IndFrequence;
  actif: boolean;
}

interface ValeurFormState {
  periode: string;
  valeur: string;
  dateMesure: string;
  commentaire: string;
  saisiParId: string;
}

@Component({
  selector: 'app-indicateurs-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './indicateurs-list.component.html',
  styleUrls: ['./indicateurs-list.component.scss']
})
export class IndicateursListComponent implements OnInit {
  canEdit = false;

  allIndicateurs: Indicateur[] = [];
  filteredIndicateurs: Indicateur[] = [];
  processus: { id: string; code: string; nom: string }[] = [];
  responsables: { id: number; nom: string }[] = [];

  searchTerm = '';
  processusFilter = '';
  // Pagination
currentPage = 1;
pageSize = 7;
totalPages = 1;

  showFormModal = false;
  showValeurModal = false;
  showDeleteModal = false;
  showDashboard = false;

  selectedIndicateur: Indicateur | null = null;
  isEdit = false;

  form: IndFormState = this.emptyForm();
  valeurForm: ValeurFormState = this.emptyValeurForm();
  formErrors: string[] = [];
  valeurErrors: string[] = [];

  constructor(private indService: IndicateurService,private authService: AuthService ) {}

  ngOnInit(): void {
     const role = this.authService.getRole();
    this.canEdit = role === 'ADMIN_ORG' || role === 'RESPONSABLE_SMQ';
    this.loadData();
  }

  loadData(): void {
    // Charger les processus
    this.indService.getProcessusList().subscribe(data => {
      this.processus = data;
    });

    // Charger les responsables
    this.indService.getResponsablesList().subscribe(data => {
      this.responsables = data;
    });

    // Charger les indicateurs
    this.indService.getIndicateurs().subscribe(data => {
      this.allIndicateurs = data;
      this.applyFilters();
    });
  }

  applyFilters(): void {
    let list = [...this.allIndicateurs];
    if (this.searchTerm.trim()) {
      const q = this.searchTerm.toLowerCase();
      list = list.filter(i =>
        i.code.toLowerCase().includes(q) ||
        i.nom.toLowerCase().includes(q) ||
        (i.methodeCalcul || '').toLowerCase().includes(q)
      );
    }
    if (this.processusFilter) {
      list = list.filter(i => i.processusId === this.processusFilter);
    }
    this.filteredIndicateurs = list;
    this.currentPage = 1;
  this.updatePagination();
  }

  onSearchChange(): void { this.applyFilters(); }
  onProcessusChange(): void { this.applyFilters(); }

  openCreate(): void {
    this.isEdit = false;
    this.form = this.emptyForm();
    this.formErrors = [];
    this.showFormModal = true;
  }

 openEdit(ind: Indicateur): void {
  this.isEdit = true;
  this.selectedIndicateur = ind;
  this.form = {
    code: ind.code,
    nom: ind.nom,
    description: ind.description || '',
    processusId: ind.processusId || '',  // ← Peut être undefined
    responsableId: String(ind.responsableId || ''),
    unite: ind.unite || '',
    methodeCalcul: ind.methodeCalcul,
    valeurCible: ind.valeurCible !== undefined ? String(ind.valeurCible) : '',
    seuilAlerte: ind.seuilAlerte !== undefined ? String(ind.seuilAlerte) : '',
    frequence: ind.frequence,
    actif: ind.statut === 'ACTIF'
  };
  // ✅ Si processusId est vide, essayer de le récupérer depuis l'objet
  if (!this.form.processusId && ind.processusId) {
    this.form.processusId = ind.processusId;
  }
  this.formErrors = [];
  this.showFormModal = true;
}

  closeForm(): void { this.showFormModal = false; this.formErrors = []; }


saveIndicateur(): void {
  this.formErrors = [];
  if (!this.form.code.trim()) this.formErrors.push('Le code est obligatoire.');
  if (!this.form.nom.trim()) this.formErrors.push('Le nom est obligatoire.');
  if (!this.form.processusId) this.formErrors.push('Le processus est obligatoire.');
  if (!this.form.methodeCalcul.trim()) this.formErrors.push('La méthode de calcul est obligatoire.');
  if (this.formErrors.length) return;

  let processusId: any = this.form.processusId;
  if (processusId === '' || processusId === null || processusId === undefined) {
    processusId = undefined;
  }

  console.log('📤 FINAL processusId:', processusId);

  if (this.isEdit && this.selectedIndicateur) {
    // Vérifier si seul le statut change
    const seulStatutChange = 
      this.form.code === this.selectedIndicateur.code &&
      this.form.nom === this.selectedIndicateur.nom &&
      this.form.description === (this.selectedIndicateur.description || '') &&
      this.form.processusId === (this.selectedIndicateur.processusId || '') &&
      this.form.responsableId === String(this.selectedIndicateur.responsableId || '') &&
      this.form.unite === (this.selectedIndicateur.unite || '') &&
      this.form.methodeCalcul === this.selectedIndicateur.methodeCalcul &&
      this.form.valeurCible === (this.selectedIndicateur.valeurCible !== undefined ? String(this.selectedIndicateur.valeurCible) : '') &&
      this.form.seuilAlerte === (this.selectedIndicateur.seuilAlerte !== undefined ? String(this.selectedIndicateur.seuilAlerte) : '') &&
      this.form.frequence === this.selectedIndicateur.frequence;

    if (seulStatutChange) {
      console.log('Seul le statut change, utilisation de updateStatut');
      this.indService.updateStatut(this.selectedIndicateur.id, this.form.actif).subscribe({
        next: () => {
          this.loadData();
          this.closeForm();
        },
        error: (err) => {
          console.error('Erreur:', err);
          this.formErrors.push('Erreur lors du changement de statut');
        }
      });
    } else {
      // ✅ CORRECTION ICI : utiliser frequenceMesure au lieu de frequence
      const dto: CreateIndicateurDto = {
        code: this.form.code,
        nom: this.form.nom,
        description: this.form.description || undefined,
        processusId: processusId,
        responsableId: this.form.responsableId ? Number(this.form.responsableId) : undefined,
        unite: this.form.unite || undefined,
        methodeCalcul: this.form.methodeCalcul,
        valeurCible: this.form.valeurCible ? Number(this.form.valeurCible) : undefined,
        seuilAlerte: this.form.seuilAlerte ? Number(this.form.seuilAlerte) : undefined,
        frequenceMesure: this.form.frequence,   // ← CHANGÉ
        actif: this.form.actif
      };
      console.log('📤 DTO complet:', JSON.stringify(dto, null, 2));
      this.indService.updateIndicateur(this.selectedIndicateur.id, dto).subscribe({
        next: () => {
          this.loadData();
          this.closeForm();
        },
        error: (err) => {
          console.error('Erreur:', err);
          this.formErrors.push('Erreur lors de la mise à jour');
        }
      });
    }
  } else {
    // Création (déjà correct)
    const dto: CreateIndicateurDto = {
      code: this.form.code,
      nom: this.form.nom,
      description: this.form.description || undefined,
      processusId: processusId,
      responsableId: this.form.responsableId ? Number(this.form.responsableId) : undefined,
      unite: this.form.unite || undefined,
      methodeCalcul: this.form.methodeCalcul,
      valeurCible: this.form.valeurCible ? Number(this.form.valeurCible) : undefined,
      seuilAlerte: this.form.seuilAlerte ? Number(this.form.seuilAlerte) : undefined,
      frequenceMesure: this.form.frequence,
      actif: this.form.actif
    };
    console.log('📤 DTO complet envoyé:', JSON.stringify(dto, null, 2));
    this.indService.createIndicateur(dto).subscribe({
      next: () => {
        this.loadData();
        this.closeForm();
      },
      error: (err) => {
        console.error('Erreur:', err);
        this.formErrors.push('Erreur lors de la création');
      }
    });
  }
}
  openValeur(ind: Indicateur): void {
    this.selectedIndicateur = ind;
    this.valeurForm = this.emptyValeurForm();
    this.valeurErrors = [];
    this.showValeurModal = true;
  }

  closeValeur(): void { this.showValeurModal = false; this.valeurErrors = []; }

  saveValeur(): void {
    this.valeurErrors = [];
    if (!this.valeurForm.periode.trim()) this.valeurErrors.push('La période est obligatoire.');
    if (this.valeurForm.valeur === '') this.valeurErrors.push('La valeur est obligatoire.');
    if (!this.valeurForm.saisiParId) this.valeurErrors.push('Le saisi par est obligatoire.');
    if (this.valeurErrors.length) return;

    const dto: CreateValeurDto = {
      periode: this.valeurForm.periode,
      valeur: Number(this.valeurForm.valeur),
      dateMesure: this.valeurForm.dateMesure,
      commentaire: this.valeurForm.commentaire || undefined,
      saisiParId: Number(this.valeurForm.saisiParId)  
    };

    this.indService.addValeur(this.selectedIndicateur!.id, dto).subscribe({
      next: () => {
        this.loadData();
        this.closeValeur();
      },
      error: (err) => {
        console.error('Erreur lors de l\'ajout de la valeur:', err);
        this.valeurErrors.push('Erreur lors de l\'ajout de la valeur');
      }
    });
  }

  openDelete(ind: Indicateur): void { this.selectedIndicateur = ind; this.showDeleteModal = true; }
  closeDelete(): void { this.showDeleteModal = false; this.selectedIndicateur = null; }

  confirmDelete(): void {
    if (!this.selectedIndicateur) return;
    this.indService.deleteIndicateur(this.selectedIndicateur.id).subscribe({
      next: () => {
        this.loadData();
        this.closeDelete();
      },
      error: (err) => {
        console.error('Erreur lors de la suppression:', err);
      }
    });
  }

  openDashboard(): void { this.showDashboard = true; }
  closeDashboard(): void { this.showDashboard = false; }

  private emptyForm(): IndFormState {
    return {
      code: '', nom: '', description: '',
      processusId: '', responsableId: '',
      unite: '', methodeCalcul: '',
      valeurCible: '', seuilAlerte: '',
      frequence: 'MENSUEL', actif: true
    };
  }

  private emptyValeurForm(): ValeurFormState {
    return {
      periode: '',
      valeur: '',
      dateMesure: '',
      commentaire: '',
      saisiParId: ''
    };
  }

  getStatutClass(statut: IndStatut): string {
    return statut === 'ACTIF' ? 'ind-badge-actif' : 'ind-badge-inactif';
  }

  getBarColor(ind: Indicateur): string {
    if (ind.derniereValeur === undefined) return '#d1d5db';
    if (ind.valeurCible !== undefined && ind.derniereValeur >= ind.valeurCible) return '#16a34a';
    if (ind.seuilAlerte !== undefined && ind.derniereValeur < ind.seuilAlerte) return '#dc2626';
    return '#d97706';
  }

  getBarWidth(ind: Indicateur): number {
    if (ind.derniereValeur === undefined || ind.valeurCible === undefined || ind.valeurCible === 0) return 0;
    return Math.min(100, Math.round((ind.derniereValeur / ind.valeurCible) * 100));
  }

  formatValeur(ind: Indicateur): string {
  if (ind.derniereValeur === undefined || ind.derniereValeur === null) {
    return '—';
  }
  return `${ind.derniereValeur}${ind.unite ? ' ' + ind.unite : ''}`;
}

  formatCible(ind: Indicateur): string {
    if (ind.valeurCible === undefined) return '—';
    return `${ind.valeurCible}${ind.unite ? ' ' + ind.unite : ''}`;
  }

  formatSeuil(ind: Indicateur): string {
    if (ind.seuilAlerte === undefined) return '—';
    return `${ind.seuilAlerte}${ind.unite ? ' ' + ind.unite : ''}`;
  }
  get paginatedIndicateurs(): Indicateur[] {
  const start = (this.currentPage - 1) * this.pageSize;
  return this.filteredIndicateurs.slice(start, start + this.pageSize);
}

get lastIndex(): number {
  const end = this.currentPage * this.pageSize;
  return end > this.filteredIndicateurs.length ? this.filteredIndicateurs.length : end;
}

pagesArray(): number[] {
  return Array(this.totalPages).fill(0).map((_, i) => i + 1);
}

updatePagination(): void {
  this.totalPages = Math.ceil(this.filteredIndicateurs.length / this.pageSize);
  if (this.currentPage > this.totalPages) this.currentPage = 1;
}
}
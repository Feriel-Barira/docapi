import { Component, Input, OnInit, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ProcessusDto, ProcessType, ProcessStatut, TypeActeur } from '../../../shared/models/process.model';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { ProcessService } from 'src/app/core/services/process.service';
import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
interface ArraySection {
  key: keyof ArrayFields;
  label: string;
  placeholder: string;
}

interface ArrayFields {
  objectifs:     string[];
  finalites:     string[];
  perimetres:    string[];
  fournisseurs:  string[];
  clients:       string[];
  donneesEntree: string[];
  donneesSortie: string[];
}

type ArrayKey = keyof ArrayFields;

interface ActeurRow {
  utilisateurId: string;
  typeActeur:    string;
}

@Component({
  selector: 'app-process-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './process-form-modal.component.html',
  styleUrls: ['./process-form-modal.component.scss']
})
export class ProcessFormModalComponent implements OnInit, OnChanges {
  originalCode: string = '';

  @Input()  process: ProcessusDto | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() save  = new EventEmitter<any>();

  processForm!: FormGroup;
  ProcessType   = ProcessType;
  ProcessStatut = ProcessStatut;
  TypeActeur    = TypeActeur;

  get isEdit(): boolean { return !!this.process; }

  actors: ActeurRow[] = [];
  expandedSections: Record<string, boolean> = {};

  kpis: string[] = [];
  pilotesDisponibles: { id: number; nom: string }[] = [];

  arrayFields: ArrayFields = {
    objectifs: [], finalites: [], perimetres: [],
    fournisseurs: [], clients: [], donneesEntree: [], donneesSortie: []
  };

  inputBuffers: Record<string, string> = {
    finalites: '', objectifs: '', perimetres: '',
    fournisseurs: '', clients: '', donneesEntree: '', donneesSortie: ''
  };

  finalistesSections: ArraySection[] = [
    { key: 'finalites', label: 'Finalités', placeholder: 'Ajouter une finalité...' }
  ];

  perimeterSections: ArraySection[] = [
    { key: 'perimetres',    label: 'Périmètres',             placeholder: 'Ajouter un périmètre...'         },
    { key: 'fournisseurs',  label: 'Fournisseurs (entrants)', placeholder: 'Ajouter un fournisseur...'       },
    { key: 'clients',       label: 'Clients (sortants)',      placeholder: 'Ajouter un client...'            },
    { key: 'donneesEntree', label: "Données d'entrée",        placeholder: "Ajouter une donnée d'entrée..."  },
    { key: 'donneesSortie', label: 'Données de sortie',       placeholder: 'Ajouter une donnée de sortie...' }
  ];

 constructor(private fb: FormBuilder, private http: HttpClient,private processService: ProcessService  ) {}

  ngOnInit(): void {
  this.buildForm();
  this.initExpandedSections();
  this.loadPilotes();  // ← AJOUTER
  if (this.process) this.loadData();
}

  ngOnChanges(changes: SimpleChanges): void {
    // Ne réagir que si le process a changé et que le formulaire existe
    if (this.processForm && changes['process'] && !changes['process'].firstChange) {
      console.log('Process changed:', this.process); // Debug
      if (this.process) {
        this.loadData(); // Recharger avec les nouvelles données
      } else {
        this.resetForm(); // Réinitialiser seulement si on passe en mode création
      }
    }
  }
loadPilotes(): void {
  const orgId = localStorage.getItem('auth_organisation_id') || '';
  const token = localStorage.getItem('auth_token') || '';
  
  this.http.get<any[]>(`${environment.apiUrl}/Users/responsables?organisationId=${orgId}`, {
    headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` })
  }).subscribe({
    next: users => {
      this.pilotesDisponibles = users.map(u => ({
        id:  u.Id  || u.id,
        nom: u.Nom || u.nom || ''
      })).filter(u => u.nom !== '');

      if (this.process) {
        this.processForm.patchValue({
          piloteId: this.extractPiloteId(this.process)
        });
      }
    },
    error: (err) => {
      console.error('Erreur chargement pilotes:', err);
    }
  });
}
  buildForm(): void {
    this.processForm = this.fb.group({
      code:        ['', [Validators.required], [this.codeUnicityValidator()]],
      name:        ['', Validators.required],
      type:        ['', Validators.required],
      piloteId:    ['', Validators.required],
      statut:      [ProcessStatut.ACTIF],
      description: ['']
    });
  }

  initExpandedSections(): void {
    // Ouvrir les sections principales par défaut pour une meilleure UX
    this.expandedSections = {
      finalites: false,
      objectifs: false,
      perimetres: false,
      fournisseurs: false,
      clients: false,
      donneesEntree: false,
      donneesSortie: false
    };
  }

  resetForm(): void {
    this.processForm.reset({ 
      code: '', 
      name: '', 
      type: '', 
      piloteId: '', 
      statut: ProcessStatut.ACTIF, 
      description: '' 
    });
    
    this.arrayFields = {
      objectifs: [], 
      finalites: [], 
      perimetres: [],
      fournisseurs: [], 
      clients: [], 
      donneesEntree: [], 
      donneesSortie: []
    };
    
    this.inputBuffers = {
      finalites: '', 
      objectifs: '', 
      perimetres: '',
      fournisseurs: '', 
      clients: '', 
      donneesEntree: '', 
      donneesSortie: ''
    };
    
    this.kpis = [];
    this.actors = [];
    
    // Réinitialiser les sections ouvertes
    this.initExpandedSections();
  }

 loadData(): void {
  if (!this.process) return;
  
  const p = this.process;
  
  this.processForm.patchValue({
    code:        p.code || '',
    name:        p.nom || p.name || '',
    type:        p.type || '',
    piloteId:    this.extractPiloteId(p),
    statut:      p.statut || ProcessStatut.ACTIF,
    description: p.description || ''
  });
  this.originalCode = this.process?.code || '';

  this.arrayFields = {
    objectifs:     [...(p.objectifs     ?? [])],
    finalites:     [...(p.finalites     ?? [])],
    perimetres:    [...(p.perimetres    ?? [])],
    fournisseurs:  [...(p.fournisseurs  ?? [])],
    clients:       [...(p.clients       ?? [])],
    donneesEntree: [...(p.donneesEntree ?? [])],
    donneesSortie: [...(p.donneesSortie ?? [])]
  };

  this.kpis = [...(p.kpis ?? p.objectifs ?? [])];

  // ← AJOUTER : charger les acteurs depuis le backend
  const token = localStorage.getItem('auth_token') || '';
 this.http.get<any[]>(`${environment.apiUrl}/Processus/${p.id}/acteurs`, {
  headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` })
}).subscribe({
  next: acteurs => {
    console.log('Acteurs reçus:', acteurs);
    this.actors = acteurs.map(a => ({
      utilisateurId:  a.UtilisateurId ? String(a.UtilisateurId) : '',
      typeActeur:    a.TypeActeur    || a.typeActeur || 'CONTRIBUTEUR'
    }));
  },
  error: (err) => { 
    console.error('Erreur acteurs:', err);
    this.actors = []; 
  }
});

  this.expandSectionsWithContent();
}
  
extractPiloteId(process: ProcessusDto): string {
  // Chercher dans la liste dynamique par nom
  const pilotName = process.pilot || '';
  if (pilotName && this.pilotesDisponibles.length > 0) {
    const found = this.pilotesDisponibles.find(p => p.nom === pilotName);
    if (found) return String(found.id);
  }
  // Fallback: chercher par PiloteId directement
  if ((process as any).PiloteId) return String((process as any).PiloteId);
  return '';
}

mapPilotNameToId(name: string): string {
  const found = this.pilotesDisponibles.find(p => p.nom === name);
  return found ? String(found.id) : '';
}
  
 
  
  expandSectionsWithContent(): void {
    // Ouvrir automatiquement les sections qui contiennent des données
    Object.keys(this.arrayFields).forEach(key => {
      const arrayKey = key as ArrayKey;
      if (this.arrayFields[arrayKey] && this.arrayFields[arrayKey].length > 0) {
        this.expandedSections[key] = true;
      }
    });
    
    if (this.kpis.length > 0) {
      this.expandedSections['objectifs'] = true;
    }
  }

  // ── Accordéon ────────────────────────────────────────────────────────
  toggleSection(key: string): void {
    this.expandedSections[key] = !this.expandedSections[key];
  }

  // ── Array items ───────────────────────────────────────────────────────
  addItemFromBuffer(key: ArrayKey): void {
    const val = (this.inputBuffers[key] ?? '').trim();
    if (!val) {
      console.warn('Empty value, not adding');
      return;
    }
    this.arrayFields[key] = [...this.arrayFields[key], val];
    this.inputBuffers[key] = '';
    console.log(`Added item to ${key}:`, this.arrayFields[key]);
  }

  addKPIFromBuffer(): void {
    const val = (this.inputBuffers['objectifs'] ?? '').trim();
    if (!val) {
      console.warn('Empty KPI, not adding');
      return;
    }
    this.kpis = [...this.kpis, val];
    this.inputBuffers['objectifs'] = '';
    console.log('Added KPI:', this.kpis);
  }

  removeItem(key: ArrayKey, index: number): void {
    this.arrayFields[key] = this.arrayFields[key].filter((_, i) => i !== index);
    console.log(`Removed item from ${key} at index ${index}`);
  }

  removeKPI(index: number): void {
    this.kpis = this.kpis.filter((_, i) => i !== index);
    console.log(`Removed KPI at index ${index}`);
  }

  // ── Acteurs ───────────────────────────────────────────────────────────
  addActor(): void {
    this.actors.push({ 
      utilisateurId: '', 
      typeActeur: 'CONTRIBUTEUR' 
    });
    console.log('Added actor, total:', this.actors.length);
  }

  removeActor(index: number): void {
    this.actors.splice(index, 1);
    console.log('Removed actor at index', index);
  }

  onActeurUserChange(index: number, id: any): void {
    this.actors[index].utilisateurId = id ? String(id) : '';
    console.log(`Actor ${index} user changed to:`, this.actors[index].utilisateurId);
  }

  // ── Save ──────────────────────────────────────────────────────────────
  onSave(): void {
    console.log('onSave called');
    
    if (this.processForm.invalid) {
      console.log('Form is invalid');
      Object.keys(this.processForm.controls)
        .forEach(k => {
          const control = this.processForm.get(k);
          if (control?.invalid) {
            console.log(`Field ${k} is invalid:`, control.errors);
            control.markAsTouched();
          }
        });
      return;
    }

    const f = this.processForm.value;
    const clean = (arr: string[]) => arr.filter(s => s && s.trim() !== '');
    
    // Nettoyer les acteurs (enlever ceux qui n'ont pas d'utilisateur)
    const cleanActors = this.actors
     .filter(a => a.utilisateurId && a.utilisateurId !== '')
  .map(a => ({ 
    utilisateurId: Number(a.utilisateurId), 
    typeActeur: a.typeActeur 
      }));

    const payload = {
      // Données de base
      code:          f.code,
      nom:           f.name,
      name:          f.name,
      type:          f.type,
      piloteId:      +f.piloteId,
      description:   f.description || '',
      statut:        f.statut,
      
      // Tableaux dynamiques
      finalites:     clean(this.arrayFields.finalites),
      objectifs:     clean(this.kpis),  // Les KPIs sont les objectifs
      perimetres:    clean(this.arrayFields.perimetres),
      fournisseurs:  clean(this.arrayFields.fournisseurs),
      clients:       clean(this.arrayFields.clients),
      donneesEntree: clean(this.arrayFields.donneesEntree),
      donneesSortie: clean(this.arrayFields.donneesSortie),
      
      // Acteurs
      acteurs: cleanActors,
      
      // Métadonnées
      kpis: clean(this.kpis)  // Garder aussi pour compatibilité
    };
    
    console.log('Saving payload:', payload);
    this.save.emit(payload);
  }

  onCancel(): void { 
    console.log('Cancel clicked');
    this.close.emit(); 
  }
  private codeUnicityValidator(): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    const value = control.value;
    if (!value || value === this.originalCode) {
      return of(null);
    }
    return this.processService.checkProcessusCode(value, this.process?.id).pipe(
      map(exists => exists ? { codeTaken: true } : null),
      catchError(() => of(null))
    );
  };
}
}
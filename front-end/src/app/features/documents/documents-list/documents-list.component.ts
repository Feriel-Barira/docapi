// src/app/features/documents/documents-list/documents-list.component.ts
import { Component, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../../core/services/document.service';
import { DocumentISO, DocTypeDocument, DocStatut, CreateDocumentDto, CreateVersionDto } from '../../../shared/models/document.model';
import { ProcessService } from 'src/app/core/services/process.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { AuthService } from '../../../core/services/auth.service';
interface DocFormState {
  code:          string;
  titre:         string;
  typeDocument:  DocTypeDocument;
  processusId:   string;
  description:   string;
  actif:         boolean;
  proceduresAssocieesIds: string[];
  // Première version
  numeroVersion:       string;
  versionStatut:       DocStatut;
  commentaireRevision: string;
  fichierPath:         string;
  etabliParId:         string;
  verifieParId:        string;
  valideParId:         string;
  dateEtablissement:   string;
  dateVerification:    string;
  dateValidation:      string;
  dateMiseEnVigueur:   string;
}

@Component({
  selector: 'app-documents-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documents-list.component.html',
  styleUrls: ['./documents-list.component.scss'],
  host: { style: 'display:flex;flex-direction:column;flex:1;overflow:hidden;min-height:0;' }
})
export class DocumentsListComponent implements OnInit, OnDestroy {
  canEdit = false;
canManageVersions = false; 

  documents: DocumentISO[]         = [];
  filteredDocuments: DocumentISO[] = [];
isNewVersion = false;
  searchTerm   = '';
  typeFilter   = '';
  statutFilter = '';
  currentPage  = 1;
  pageSize     = 10;

  // Modals
  showVersionsModal = false;
  showFormModal     = false;
  showDeleteModal   = false;

  selectedDocument: DocumentISO | null = null;
  isEdit = false;
  form: DocFormState = this.emptyForm();
  loading = false;

  private nextId = 100;

  processusDisponibles: { id: string; code: string; nom: string; }[] = [];

  proceduresDisponibles = [
    { id: '1', code: 'PRO-001', titre: 'Maîtrise des documents'       },
    { id: '2', code: 'PRO-002', titre: 'Maîtrise des enregistrements' },
    { id: '3', code: 'PRO-003', titre: 'Audit interne'                },
    { id: '4', code: 'PRO-004', titre: 'Qualification fournisseurs'   },
    { id: '5', code: 'PRO-005', titre: 'Plan de formation'            },
    { id: '6', code: 'PRO-006', titre: 'Contrôle qualité final'       },
  ];

  responsablesDisponibles: { id: string; nom: string; initiales: string; couleur: string; }[] = [];

  constructor(
  private documentService: DocumentService,
  private processService:  ProcessService,
  private notification:    NotificationService,
  private http:            HttpClient,
  private renderer:        Renderer2,
   private authService: AuthService
) {
  const role = this.authService.getRole();
  this.canEdit = role === 'ADMIN_ORG' || role === 'RESPONSABLE_SMQ';
  this.canManageVersions = this.canEdit; 
}

 ngOnInit(): void {
  this.loadProcessus();
  this.loadResponsables();
  this.loadDocuments();
}
  ngOnDestroy(): void { this.renderer.removeClass(document.body, 'modal-open'); }
  private loadProcessus(): void {
  this.processService.getProcesses().subscribe({
    next: data => {
      this.processusDisponibles = data.map(p => ({ id: p.id, code: p.code, nom: p.nom }));
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
      const colors = ['#166534','#6b21a8','#7c2d12','#155e75','#92400e','#1e3a5f'];
      this.responsablesDisponibles = users.map((u, i) => {
        const prenom = u.Prenom || u.prenom || '';
        const nom    = u.Nom    || u.nom    || u.Username || '';
        return {
          id:        String(u.Id || u.id),
          nom:       `${prenom} ${nom}`.trim() || u.Username || '',
          initiales: prenom && nom ? `${prenom[0]}${nom[0]}`.toUpperCase() : (u.Username || '??').substring(0,2).toUpperCase(),
          couleur:   colors[i % colors.length]
        };
      });
    },
    error: () => {}
  });
}

  loadDocuments(): void {
    this.loading = true;
    this.documentService.getDocuments().subscribe({
      next: data => { this.documents = data; this.applyFilters(); this.loading = false; },
      error: ()   => { this.loading = false; }
    });
  }

  // ── Filtres ───────────────────────────────────────────────────────────
  applyFilters(): void {
    let f = [...this.documents];
    if (this.searchTerm) {
      const t = this.searchTerm.toLowerCase();
      f = f.filter(d => d.titre.toLowerCase().includes(t) || d.code.toLowerCase().includes(t));
    }
    if (this.typeFilter)   f = f.filter(d => d.typeDocument === this.typeFilter);
    if (this.statutFilter) f = f.filter(d => d.derniereVersionStatut === this.statutFilter);
    this.filteredDocuments = f;
    this.currentPage = 1;
  }

  setTypeFilter(t: string): void   { this.typeFilter   = t; this.applyFilters(); }
  setStatutFilter(s: string): void  { this.statutFilter = s; this.applyFilters(); }
  onSearchChange(v: string): void   { this.searchTerm   = v; this.applyFilters(); }

  // ── Pagination ────────────────────────────────────────────────────────
  get paginatedDocuments(): DocumentISO[] {
    const s = (this.currentPage - 1) * this.pageSize;
    return this.filteredDocuments.slice(s, s + this.pageSize);
  }
  get totalPages(): number { return Math.ceil(this.filteredDocuments.length / this.pageSize); }
  get lastIndex(): number  { return Math.min(this.currentPage * this.pageSize, this.filteredDocuments.length); }
  pagesArray(): number[]   { return Array.from({ length: this.totalPages }, (_, i) => i + 1); }

  // ── Helpers ───────────────────────────────────────────────────────────
  getStatutClass(s?: DocStatut): string {
    switch (s) {
      case 'VALIDE':      return 'conforme';
      case 'EN_REVISION': return 'revision';
      case 'BROUILLON':   return 'blue';
      case 'OBSOLETE':    return 'perime';
      default:            return '';
    }
  }

  getStatutLabel(s?: DocStatut): string {
    switch (s) {
      case 'VALIDE':      return '● Approuvé';
      case 'EN_REVISION': return '⏳ En révision';
      case 'BROUILLON':   return '✎ Brouillon';
      case 'OBSOLETE':    return '✕ Obsolète';
      default:            return '—';
    }
  }

  getTypeClass(t: DocTypeDocument): string { return t === 'REFERENCE' ? 'blue' : 'green-t'; }
  getTypeLabel(t: DocTypeDocument): string { return t === 'REFERENCE' ? 'Reference' : 'Travail'; }

  getProcessusCode(d: DocumentISO): string {
    if (d.processusCode) return d.processusCode;
    return this.processusDisponibles.find(p => p.id === d.processusId)?.code ?? '—';
  }

  isProcSelected(id: string): boolean { return this.form.proceduresAssocieesIds.includes(id); }
  toggleProc(id: string): void {
    const i = this.form.proceduresAssocieesIds.indexOf(id);
    if (i === -1) this.form.proceduresAssocieesIds.push(id);
    else          this.form.proceduresAssocieesIds.splice(i, 1);
  }

  // ── Modal control ─────────────────────────────────────────────────────
  private openM(): void  { this.renderer.addClass(document.body, 'modal-open'); }
  private closeM(): void { this.renderer.removeClass(document.body, 'modal-open'); }

  // ── Modal Versions & Traçabilité ──────────────────────────────────────
 openVersions(d: DocumentISO): void {
  // ✅ Recharger le document complet pour avoir toutes les versions
  this.documentService.getDocument(d.id).subscribe({
    next: (fullDocument) => {
      this.selectedDocument = fullDocument;
      this.showVersionsModal = true;
      this.openM();
    },
    error: () => {
      // Fallback sur le document existant
      this.selectedDocument = d;
      this.showVersionsModal = true;
      this.openM();
    }
  });
}
  closeVersions(): void              { this.showVersionsModal = false; this.selectedDocument = null; this.closeM(); }

  // ── Modal Formulaire ──────────────────────────────────────────────────
  openCreate(): void {
    if (!this.canEdit) return;
    this.isEdit = false; this.form = this.emptyForm();
    this.showFormModal = true; this.openM();
  }

 openEdit(d: DocumentISO): void {
   if (!this.canEdit) return;
  this.isEdit = true;
  this.selectedDocument = d;
  console.log('📋 Document chargé - valeur actif:', d.actif); 
  // Récupérer la dernière version
  const lastVersion = d.versions && d.versions.length > 0 ? d.versions[0] : null;
  
  this.form = {
    code: d.code,
    titre: d.titre,
    typeDocument: d.typeDocument,
    processusId: d.processusId ?? '',
    description: d.description ?? '',
    actif: d.actif,
    proceduresAssocieesIds: [...(d.proceduresAssocieesIds ?? [])],
    
    // ✅ Charger les infos de la dernière version
    numeroVersion: lastVersion?.numeroVersion || 'v1.0',
    versionStatut: lastVersion?.statut || 'BROUILLON',
    commentaireRevision: lastVersion?.commentaireRevision || '',
    fichierPath: lastVersion?.fichierPath || '',
    
    // ✅ Charger les responsables depuis la version
    etabliParId: lastVersion?.etabliParId?.toString() || '',
    verifieParId: lastVersion?.verifieParId?.toString() || '',
    valideParId: lastVersion?.valideParId?.toString() || '',
    
    // ✅ Charger les dates depuis la version
    dateEtablissement: lastVersion?.dateEtablissement?.split('T')[0] || '',
    dateVerification: lastVersion?.dateVerification?.split('T')[0] || '',
    dateValidation: lastVersion?.dateValidation?.split('T')[0] || '',
    dateMiseEnVigueur: lastVersion?.dateMiseEnVigueur?.split('T')[0] || ''
  };
   console.log('📝 Formulaire initialisé - actif:', this.form.actif);
  
  this.showFormModal = true;
  this.openM();
}

closeForm(): void {
  this.showFormModal = false;
  this.isNewVersion = false;  // ← Réinitialiser
  this.isEdit = false;
  this.selectedDocument = null;
  this.closeM();
}
// Dans documents-list.component.ts, remplacez la méthode saveDocument() par celle-ci :
saveDocument(): void {
  if (!this.form.code || !this.form.titre) {
    this.notification.showError('Veuillez remplir le code et le titre');
    return;
  }

  // ✅ CAS 1: NOUVELLE VERSION
  if (this.isNewVersion && this.selectedDocument) {
    const versionDto: CreateVersionDto = {
      numeroVersion: this.form.numeroVersion || '1.0',
      statut: this.form.versionStatut,
      commentaireRevision: this.form.commentaireRevision || '',
      fichierPath: this.form.fichierPath,
      // ⚠️ Envoyer directement les strings, pas de parseInt()
      etabliParId: this.form.etabliParId || undefined,
      verifieParId: this.form.verifieParId || undefined,
      valideParId: this.form.valideParId || undefined,
     dateEtablissement: this.form.dateEtablissement 
    ? new Date(this.form.dateEtablissement).toISOString()  // ← ISO complet avec heure
    : new Date().toISOString(),
      dateVerification: this.form.dateVerification || undefined,
      dateValidation: this.form.dateValidation || undefined,
      dateMiseEnVigueur: this.form.dateMiseEnVigueur || undefined
    };
    
    console.log('📝 Envoi nouvelle version:', versionDto);
    
    this.documentService.addVersion(this.selectedDocument.id, versionDto).subscribe({
      next: () => {
        this.notification.showSuccess('Nouvelle version créée avec succès !');
        this.loadDocuments();
        this.closeForm();
        this.isNewVersion = false;
        this.selectedDocument = null;
      },
      error: (err) => {
        console.error('❌ Erreur création version:', err);
        this.notification.showError(err.error?.message || 'Erreur lors de la création de la version.');
      }
    });
    return;
  }

  // ✅ CAS 2: MODIFICATION (uniquement métadonnées)
  if (this.isEdit && this.selectedDocument) {
    const updateDto: any = {
      code: this.form.code,
      titre: this.form.titre,
      typeDocument: this.form.typeDocument,
      description: this.form.description,
      processusId: this.form.processusId || null,
      actif: this.form.actif
    };
     console.log('💾 Envoi mise à jour - actif:', this.form.actif);
    
    this.documentService.updateDocument(this.selectedDocument.id, updateDto).subscribe({
      next: () => {
        this.notification.showSuccess('Document modifié avec succès !');
        this.loadDocuments();
        this.closeForm();
      },
      error: (err) => {
        console.error('Erreur updateDocument:', err);
        this.notification.showError(err.error?.message || 'Erreur lors de la modification.');
      }
    });
    return;
  }

  // ✅ CAS 3: NOUVEAU DOCUMENT (CRÉATION)
  const newDocumentDto: CreateDocumentDto = {
    code: this.form.code,
    titre: this.form.titre,
    typeDocument: this.form.typeDocument,
    processusId: this.form.processusId || undefined,
    description: this.form.description || undefined,
    actif: this.form.actif,
    proceduresAssocieesIds: this.form.proceduresAssocieesIds,
    versionInitiale: {
      numeroVersion: this.form.numeroVersion || '1.0',
      statut: this.form.versionStatut,
      commentaireRevision: this.form.commentaireRevision || undefined,
      fichierPath: this.form.fichierPath || undefined,
      // ⚠️ Envoyer directement les strings, pas de parseInt()
      etabliParId: this.form.etabliParId || undefined,
      verifieParId: this.form.verifieParId || undefined,
      valideParId: this.form.valideParId || undefined,
     dateEtablissement: this.form.dateEtablissement 
    ? new Date(this.form.dateEtablissement).toISOString()  // ← ISO complet avec heure
    : new Date().toISOString(),
      dateVerification: this.form.dateVerification || undefined,
      dateValidation: this.form.dateValidation || undefined,
      dateMiseEnVigueur: this.form.dateMiseEnVigueur || undefined
    }
  };
  
  console.log('📝 Création nouveau document:', JSON.stringify(newDocumentDto, null, 2));
  
  this.documentService.createDocument(newDocumentDto).subscribe({
    next: (response) => {
      console.log('✅ Document créé:', response);
      this.notification.showSuccess('Document créé avec succès !');
      this.loadDocuments();
      this.closeForm();
    },
    error: (err) => {
      console.error('❌ Erreur création:', err);
      this.notification.showError(err.error?.message || 'Erreur lors de la création du document.');
    }
  });
}
mettreEnObsolete(): void {
  if (!this.canManageVersions) return;
  if (!this.selectedDocument?.versions?.length) return;
  const lastVersion = this.selectedDocument.versions[0];
  if (!lastVersion?.id) return;

  this.documentService.archiverVersion(lastVersion.id).subscribe({
    next: () => {
      this.notification.showSuccess('Document archivé avec succès !');
      this.loadDocuments();
      this.closeVersions();
    },
    error: (err) => {
      console.error('Erreur archiverVersion:', err);
      this.notification.showError('Erreur lors de l\'archivage.');
    }
  });
}
openNewVersion(): void {
  if (!this.canManageVersions) return;
  if (!this.selectedDocument) return;

  const lastVersion = this.selectedDocument.versions?.[0];
  // ⚠️ Incrémentation correcte (sans 'v' pour le backend)
  const newVersionNumber = this.incrementVersion(lastVersion?.numeroVersion || this.selectedDocument.derniereVersion || '1.0');

  this.isEdit = false;
  this.isNewVersion = true;

  this.form = {
    ...this.emptyForm(),
    code: this.selectedDocument.code,
    titre: this.selectedDocument.titre,
    typeDocument: this.selectedDocument.typeDocument,
    processusId: this.selectedDocument.processusId ?? '',
    description: this.selectedDocument.description ?? '',
    actif: this.selectedDocument.actif,
    proceduresAssocieesIds: [...(this.selectedDocument.proceduresAssocieesIds ?? [])],
    
    numeroVersion: newVersionNumber,
    versionStatut: 'BROUILLON',  // Une nouvelle version commence toujours en BROUILLON
    commentaireRevision: '',
    fichierPath: '',
    
    etabliParId: '',
    verifieParId: '',
    valideParId: '',
    
    dateEtablissement: '',
    dateVerification: '',
    dateValidation: '',
    dateMiseEnVigueur: ''
  };

  this.showFormModal = true;
  this.openM();
}

private incrementVersion(currentVersion: string): string {
  if (!currentVersion) return '1.0';
  
  // Support les formats "1.0", "v1.0", "1.0.0"
  const match = currentVersion.match(/(\d+)\.(\d+)/);
  if (match) {
    const major = parseInt(match[1]);
    const minor = parseInt(match[2]);
    return `${major}.${minor + 1}`;
  }
  return '1.1';
}
// Remplacer viewDocument()
viewDocument(doc: DocumentISO): void {
  const lastVersion = doc.versions?.[0];
  if (!lastVersion?.id) {
    this.notification.showWarning('Aucune version disponible');
    return;
  }
  
  const fileUrl = `${environment.apiUrl}/Document/preview/${lastVersion.id}`;
  window.open(fileUrl, '_blank');
}

// Ajouter downloadFile()
downloadFile(version: any): void {
  if (!version?.id) {
    this.notification.showWarning('Version non trouvée');
    return;
  }

  const token = localStorage.getItem('auth_token');
  const fileUrl = `${environment.apiUrl}/Document/download/${version.id}`;
  
  this.http.get(fileUrl, {
    responseType: 'blob',
    headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` })
  }).subscribe({
    next: (blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = version.fichierPath?.split('/').pop() || 'document';
      a.click();
      window.URL.revokeObjectURL(url);
      this.notification.showSuccess('Téléchargement démarré');
    },
    error: (err) => {
      console.error('Erreur téléchargement:', err);
      this.notification.showError('Erreur lors du téléchargement');
    }
  });
}

// Ajouter previewFile()
previewFile(version: any): void {
  if (!version?.id) {
    this.notification.showWarning('Version non trouvée');
    return;
  }

  const fileUrl = `${environment.apiUrl}/Document/preview/${version.id}`;
  window.open(fileUrl, '_blank');
}
onFileSelected(event: any): void {
  const file = event.target.files[0];
  if (!file) return;
  
  // Uploader le fichier vers le serveur
  const formData = new FormData();
  formData.append('file', file);
  
  this.http.post(`${environment.apiUrl}/Document/upload`, formData).subscribe({
    next: (response: any) => {
      // Une fois uploadé, stocker le nom retourné
      this.form.fichierPath = response.fileName;
      //this.notification.showSuccess('Fichier uploadé avec succès');
    },
    error: (err) => {
      this.notification.showError('Erreur lors de l\'upload');
    }
  });
}
  // ── Modal Suppression ─────────────────────────────────────────────────
  openDelete(d: DocumentISO): void { if (!this.canEdit) return; this.selectedDocument = d; this.showDeleteModal = true; this.openM(); }
  closeDelete(): void              { this.showDeleteModal = false; this.selectedDocument = null; this.closeM(); }
  confirmDelete(): void {
  if (!this.selectedDocument) return;
  this.documentService.deleteDocument(this.selectedDocument.id).subscribe({
    next: () => {
      this.notification.showSuccess('Document supprimé avec succès !');
      this.loadDocuments();
      this.closeDelete();
    },
    error: () => this.notification.showError('Erreur lors de la suppression.')
  });
}


  private emptyForm(): DocFormState {
  return {
    code: '', titre: '', typeDocument: 'REFERENCE', processusId: '',
    description: '', actif: true, proceduresAssocieesIds: [],
    numeroVersion: '1.0',  // ← sans 'v'
    versionStatut: 'BROUILLON',
    commentaireRevision: '', fichierPath: '',
    etabliParId: '', verifieParId: '', valideParId: '',
    dateEtablissement: '',
    dateVerification: '', dateValidation: '', dateMiseEnVigueur: ''
  };
}
// ✅ Vérifier si on peut mettre en obsolète
canMettreEnObsolete(): boolean {
  if (!this.canManageVersions) return false;
  if (!this.selectedDocument?.versions?.length) return false;
  
  const lastVersion = this.selectedDocument.versions[0];
  // On peut mettre en obsolète seulement si la dernière version est VALIDE
  return lastVersion?.statut === 'VALIDE';
}
// Nouvelle méthode pour formater les noms
formatNomCourt(nomComplet: string): string {
  if (!nomComplet) return '';
  
  const parts = nomComplet.trim().split(' ');
  if (parts.length === 1) {
    return parts[0];  // Un seul mot, retourner tel quel
  }
  
  // "Prénom Nom" devient "Prénom.N"
  const prenom = parts[0];
  const initialeNom = parts[parts.length - 1][0].toUpperCase();
  return `${prenom}.${initialeNom}`;
}
}
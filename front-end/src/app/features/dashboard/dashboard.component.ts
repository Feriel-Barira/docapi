import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProcessService } from '../../core/services/process.service';
import { NonConformiteService } from '../../core/services/non-conformite.service';
import { DocumentService } from '../../core/services/document.service';
import { IndicateurService } from '../../core/services/indicateur.service';
import { AuditService } from '../../core/services/audit.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  // Données principales
  processes: any[] = [];
  nonConformities: any[] = [];
  documentsRecents: any[] = [];
  indicateursAlertes: any[] = [];

  // Statistiques globales
  stats = {
    totalProcessus: 0,
    processusActifs: 0,
    totalProcedures: 0,
    totalDocuments: 0,
    documentsValides: 0,
    documentsEnAttente: 0,
    totalNC: 0,
    ncOuvertes: 0,
    ncAnalyse: 0,
    ncActionEnCours: 0,
    ncCloturees: 0,
    totalIndicateurs: 0,
    indicateursActifs: 0,
    tauxConformiteGlobal: 0
  };

  loading = true;
  error = '';

  private avatarColors = [
    '#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6',
    '#ec4899', '#06b6d4', '#84cc16', '#f97316', '#6366f1'
  ];

  // Données pour le calcul de conformité
  private allAudits: any[] = [];
  private allIndicateurs: any[] = [];
  private allNCs: any[] = [];

  constructor(
    private processService: ProcessService,
    private ncService: NonConformiteService,
    private documentService: DocumentService,
    private indicateurService: IndicateurService,
    private auditService: AuditService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
      // ✅ Forcer loading = false après 3 secondes maximum
    setTimeout(() => { this.loading = false; }, 3000);


    // 1. CHARGEMENT DES PROCESSUS
    this.processService.getProcesses().subscribe({
      next: (processData) => {
        const actifs = processData.filter(p => p.statut === 'ACTIF');
        this.stats.totalProcessus = processData.length;
        this.stats.processusActifs = actifs.length;

        this.processes = processData.slice(0, 5).map(p => ({  // ← TOUS les processus pas seulement actifs
          id: p.id,
          code: p.code,
          name: p.nom || p.name,
          type: p.type,
          statut: p.statut,
          pilot: p.pilot || 'Non défini',
          conformityScore: 0
        }));

        this.loadAuditsAndCalculate();
      },
      error: (err) => {
        console.error('Erreur processus:', err);
        this.loading = false;
      }
    });

    // 2. CHARGEMENT DES NON-CONFORMITÉS
    this.ncService.getNonConformites().subscribe({
      next: (data) => {
        this.allNCs = data;
        this.stats.totalNC = data.length;
        this.stats.ncOuvertes = data.filter(nc => nc.statut === 'OUVERTE').length;
        this.stats.ncAnalyse = data.filter(nc => nc.statut === 'ANALYSE').length;
        this.stats.ncActionEnCours = data.filter(nc => nc.statut === 'ACTION_EN_COURS').length;
        this.stats.ncCloturees = data.filter(nc => nc.statut === 'CLOTUREE').length;

        const openNC = data.filter(nc => nc.statut !== 'CLOTUREE');
        this.nonConformities = openNC.slice(0, 5).map(nc => ({
          id: nc.id,
          ref: nc.reference,
          description: nc.description,
          processCode: nc.processusCode || '—',
          priority: nc.gravite,
          statut: nc.statut,
          progress: nc.avancementAC || 0,
          responsible: nc.responsableNom || 'Non assigné',
          dateDetection: nc.dateDetection
        }));

        this.recalculateScores();
      },
      error: (err) => console.error('Erreur NC:', err)
    });

    // 3. CHARGEMENT DES DOCUMENTS
    this.documentService.getDocuments().subscribe({
      next: (data) => {
        const actifs = data.filter(d => d.actif === true);
        const enAttente = data.filter(d => d.derniereVersionStatut === 'EN_REVISION');

        this.stats.totalDocuments = data.length;
        this.stats.documentsValides = actifs.length;
        this.stats.documentsEnAttente = enAttente.length;

        this.documentsRecents = data.slice(0, 5).map(d => ({
          id: d.id,
          code: d.code,
          titre: d.titre,
          version: d.derniereVersion,
          statut: d.derniereVersionStatut,
          date: d.dateMiseAJour
        }));
      },
      error: (err) => console.error('Erreur documents:', err)
    });

    // 4. CHARGEMENT DES INDICATEURS
    this.indicateurService.getIndicateurs().subscribe({
      next: (data) => {
        this.allIndicateurs = data;
        const actifs = data.filter(i => i.statut === 'ACTIF');
        this.stats.totalIndicateurs = data.length;
        this.stats.indicateursActifs = actifs.length;

        this.indicateursAlertes = actifs
          .filter(i => {
            const valeur = i.derniereValeur;
            const cible = i.valeurCible;
            return valeur !== undefined && cible !== undefined && cible > 0 && valeur < cible;
          })
          .slice(0, 5)
          .map(i => {
            const cible = i.valeurCible!;
            const valeur = i.derniereValeur!;
            const ecart = Math.round((cible - valeur) / cible * 100);
            return {
              id: i.id,
              code: i.code,
              nom: i.nom,
              valeur: valeur,
              cible: cible,
              unite: i.unite || '',
              ecart: ecart
            };
          });

        this.recalculateScores();
      },
      error: (err) => console.error('Erreur indicateurs:', err)
    });
  }

  private loadAuditsAndCalculate(): void {
    this.auditService.getAudits().subscribe({
      next: (data) => {
        this.allAudits = data;
        this.recalculateScores();
      },
      error: (err) => {
        console.error('Erreur audits:', err);
        this.recalculateScores();
      }
    });
  }

  private recalculateScores(): void {
    if (this.processes.length === 0) return;

    this.processes = this.processes.map(process => ({
      ...process,
      conformityScore: this.calculateRealConformityScore(process)
    }));

    this.calculateGlobalConformity();
    this.loading = false;
  }

  /**
   * Calcul réel de la conformité sans valeurs par défaut
   * - Points de contrôle (poids 40%)
   * - Indicateurs (poids 30%)
   * - Non-conformités clôturées (poids 30%)
   */
private calculateRealConformityScore(process: any): number {
  if (process.statut === 'INACTIF') return 0;

  console.log(`=== CALCUL POUR ${process.code} - ${process.name} ===`);
  
  let totalWeight = 0;
  let totalScore = 0;

  // 1. Points de contrôle
  const processAudits = this.allAudits.filter(a => a.processusId === process.id);
  console.log(`  Audits trouvés: ${processAudits.length}`);
  console.log(`  Audits:`, processAudits.map(a => ({ 
    id: a.id, 
    conforme: a.derniereEvaluation?.conforme 
  })));
  
  if (processAudits.length > 0) {
    const conformes = processAudits.filter(a => 
      a.derniereEvaluation !== undefined && a.derniereEvaluation.conforme === true
    ).length;
    console.log(`  Conformes: ${conformes}/${processAudits.length}`);
    const score = (conformes / processAudits.length) * 100;
    console.log(`  Score PC: ${score}% × 0.4 = ${score * 0.4}`);
    totalScore += score * 0.4;
    totalWeight += 0.4;
  }

  // 2. Indicateurs
  const processIndicators = this.allIndicateurs.filter(i => i.processusId === process.id && i.statut === 'ACTIF');
  const indicatorsWithValues = processIndicators.filter(i => 
    i.derniereValeur !== undefined && i.valeurCible !== undefined && i.valeurCible > 0
  );
  console.log(`  Indicateurs avec valeurs: ${indicatorsWithValues.length}`);
  
  if (indicatorsWithValues.length > 0) {
    const atteints = indicatorsWithValues.filter(i => i.derniereValeur >= i.valeurCible).length;
    console.log(`  Atteints: ${atteints}/${indicatorsWithValues.length}`);
    const score = (atteints / indicatorsWithValues.length) * 100;
    console.log(`  Score IND: ${score}% × 0.3 = ${score * 0.3}`);
    totalScore += score * 0.3;
    totalWeight += 0.3;
  }

  // 3. Non-conformités
  const processNCs = this.allNCs.filter(nc => nc.processusId === process.id);
  console.log(`  NC trouvées: ${processNCs.length}`);
  
  if (processNCs.length > 0) {
    const cloturees = processNCs.filter(nc => nc.statut === 'CLOTUREE').length;
    console.log(`  Clôturées: ${cloturees}/${processNCs.length}`);
    const score = (cloturees / processNCs.length) * 100;
    console.log(`  Score NC: ${score}% × 0.3 = ${score * 0.3}`);
    totalScore += score * 0.3;
    totalWeight += 0.3;
  }

  console.log(`  TotalWeight: ${totalWeight}, TotalScore: ${totalScore}`);
  
  if (totalWeight === 0) {
    console.log(`  => Aucune donnée, retour 0`);
    return 0;
  }

  const result = Math.round(totalScore / totalWeight);
  console.log(`  => Score final: ${result}%`);
  return result;
}

  private calculateGlobalConformity(): void {
    if (this.processes.length === 0) {
      this.stats.tauxConformiteGlobal = 0;
      return;
    }

    // Ne prendre que les processus avec score > 0 pour la moyenne
    const processesWithScore = this.processes.filter(p => p.conformityScore > 0);
    if (processesWithScore.length === 0) {
      this.stats.tauxConformiteGlobal = 0;
      return;
    }

    const total = processesWithScore.reduce((sum, p) => sum + p.conformityScore, 0);
    this.stats.tauxConformiteGlobal = Math.round(total / processesWithScore.length);
  }

  // ========== HELPERS POUR L'AFFICHAGE ==========

  getProcessTypeLabel(type: string): string {
    switch (type) {
      case 'PILOTAGE': return 'Pilotage';
      case 'REALISATION': return 'Réalisation';
      case 'SUPPORT': return 'Support';
      default: return type || '—';
    }
  }

  getProcessTypeShort(type: string): string {
    return this.getProcessTypeLabel(type);
  }

  getProcessTypeClass(type: string): string {
    switch (type) {
      case 'PILOTAGE': return 'type-pilotage';
      case 'REALISATION': return 'type-realisation';
      case 'SUPPORT': return 'type-support';
      default: return '';
    }
  }

  getConformityColor(score: number): string {
    if (score === 0) return '#94a3b8';  // Gris pour pas de données
    if (score >= 80) return '#22c55e';
    if (score >= 60) return '#eab308';
    return '#ef4444';
  }

  getScoreColor(score: number): string {
    return this.getConformityColor(score);
  }

  getStatutClass(statut: string): string {
    switch (statut) {
      case 'ACTIF': return 'status-active';
      case 'INACTIF': return 'status-inactive';
      case 'VALIDE': return 'status-valid';
      case 'BROUILLON': return 'status-draft';
      case 'OBSOLETE': return 'status-obsolete';
      case 'EN_REVISION': return 'status-review';
      default: return '';
    }
  }

  getStatusClass(statut: string): string {
    return this.getStatutClass(statut);
  }

  getStatutIcon(statut: string): string {
    switch (statut) {
      case 'ACTIF': return '●';
      case 'INACTIF': return '○';
      case 'VALIDE': return '✓';
      case 'BROUILLON': return '✎';
      case 'OBSOLETE': return '✕';
      case 'EN_REVISION': return '⏳';
      default: return '●';
    }
  }

  getStatusIcon(statut: string): string {
    return this.getStatutIcon(statut);
  }

  getNCStatutLabel(statut: string): string {
    switch (statut) {
      case 'OUVERTE': return 'Ouverte';
      case 'ANALYSE': return 'En analyse';
      case 'ACTION_EN_COURS': return 'Action en cours';
      case 'CLOTUREE': return 'Clôturée';
      default: return statut;
    }
  }

  getPriorityClass(gravite: string): string {
    switch (gravite) {
      case 'CRITIQUE': return 'priority-critical';
      case 'MAJEURE': return 'priority-major';
      case 'MINEURE': return 'priority-minor';
      default: return '';
    }
  }

  getPriorityIcon(gravite: string): string {
    switch (gravite) {
      case 'CRITIQUE': return '🔴';
      case 'MAJEURE': return '🟠';
      case 'MINEURE': return '🟡';
      default: return '⚪';
    }
  }

  getProgressColor(progress: number): string {
    if (progress >= 80) return '#22c55e';
    if (progress >= 50) return '#eab308';
    return '#ef4444';
  }

  getPilotColor(pilotName: string): string {
    if (!pilotName) return '#94a3b8';
    let hash = 0;
    for (let i = 0; i < pilotName.length; i++) {
      hash = ((hash << 5) - hash) + pilotName.charCodeAt(i);
      hash |= 0;
    }
    return this.avatarColors[Math.abs(hash) % this.avatarColors.length];
  }

  getInitiales(nom: string): string {
    if (!nom) return '?';
    const parts = nom.trim().split(' ');
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    }
    return nom.substring(0, 2).toUpperCase();
  }

  formatDate(date: string): string {
    if (!date) return '—';
    try {
      return new Date(date).toLocaleDateString('fr-FR');
    } catch {
      return date;
    }
  }
}
import { Routes } from '@angular/router';
import { authGuard, loginGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [loginGuard],
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'processus',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR', 'UTILISATEUR'] },
        loadComponent: () => import('./features/processus/processus-list/processus-list.component')
          .then(m => m.ProcessusListComponent)
      },
      {
        path: 'procedures',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR', 'UTILISATEUR'] },
        loadComponent: () => import('./features/procedures/procedures-list/procedures-list.component')
          .then(m => m.ProceduresListComponent)
      },
      {
        path: 'documents',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR', 'UTILISATEUR'] },
        loadComponent: () => import('./features/documents/documents-list/documents-list.component')
          .then(m => m.DocumentsListComponent)
      },
      {
        path: 'non-conformites',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR'] },
        loadComponent: () => import('./features/non-conformites/non-conformites-list/non-conformites.component')
          .then(m => m.NonConformitesListComponent)
      },
      {
        path: 'indicateurs',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR'] },
        loadComponent: () => import('./features/indicateurs/indicateurs-list/indicateurs-list.component')
          .then(m => m.IndicateursListComponent)
      },
      {
        path: 'audits',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR'] },
        loadComponent: () => import('./features/audits/audits-list/audits-list.component')
          .then(m => m.AuditsListComponent)
      },
      {
        path: 'parametres',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG'] },
        loadComponent: () => import('./features/parametres/parametres.component')
          .then(m => m.ParametresComponent)
      },
      {
        path: 'users',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG'] },
        loadComponent: () => import('./features/users/users-list/users-list.component')
          .then(m => m.UsersListComponent)
      },
      {
        path: 'enregistrements',
        canActivate: [roleGuard],
        data: { roles: ['ADMIN_ORG', 'RESPONSABLE_SMQ', 'AUDITEUR', 'UTILISATEUR'] },
        loadComponent: () => import('./features/enregistrements/enregistrement.component')
          .then(m => m.EnregistrementComponent)
      },
      {
        path: 'unauthorized',
        loadComponent: () => import('./features/unauthorized/unauthorized.component')
          .then(m => m.UnauthorizedComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];
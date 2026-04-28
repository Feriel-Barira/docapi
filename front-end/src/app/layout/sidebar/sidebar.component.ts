import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
   isAdmin = false;
   isSMQ = false;
  isAuditeur = false;
  isUtilisateur = false;
  constructor(private router: Router,private authService: AuthService) {
    const role = this.authService.getRole();
    this.isAdmin = role === 'ADMIN_ORG';
    this.isSMQ = role === 'RESPONSABLE_SMQ';
    this.isAuditeur = role === 'AUDITEUR';
    this.isUtilisateur = role === 'UTILISATEUR';
  }
}
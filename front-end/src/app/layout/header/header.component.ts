import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {

  currentUser$ = this.authService.currentUser$;

  get currentUser() {
    return this.authService.getCurrentUser();
  }

  constructor(private authService: AuthService) {}

 getInitials(): string {
  const user = this.currentUser;
  if (!user) return 'U';

  // Cas ADMIN : première lettre du username (même si prenom/nom existent)
  if (user.role === 'ADMIN_ORG') {
    const username = user.username ?? '';
    return username.charAt(0).toUpperCase();
  }

  // Cas utilisateur normal : prénom + nom si disponibles
  if (user.prenom && user.nom) {
    return (user.prenom.charAt(0) + user.nom.charAt(0)).toUpperCase();
  }

  // Fallback (si pas de prenom/nom) : première lettre du username
  const name = user.username ?? '';
  return name.charAt(0).toUpperCase();
}

  logout(): void {
    this.authService.logout();
  }
  get displayName(): string {
  const user = this.currentUser;
  if (!user) return '';
  if (user.role === 'ADMIN_ORG') {
    return user.username;
  }
  return `${user.prenom || ''} ${user.nom || ''}`.trim() || user.username;
}
}
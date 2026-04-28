import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError(error => {
      // Ne pas afficher d'erreur pour les appels auth (gérés par authInterceptor)
      if (req.url.includes('/auth/login') || req.url.includes('/auth/refresh')) {
        return throwError(() => error);
      }

      // Ne pas afficher d'erreur 401 ici : l'authInterceptor s'en charge (refresh)
      if (error.status === 401) {
        return throwError(() => error);
      }

      let errorMessage = 'Une erreur est survenue';

      if (error.error instanceof ErrorEvent) {
        errorMessage = `Erreur : ${error.error.message}`;
      } else {
        switch (error.status) {
          case 403:  errorMessage = 'Accès interdit.'; break;
          case 404:  errorMessage = 'Ressource non trouvée.'; break;
          case 409:  errorMessage = error.error?.message || 'Conflit de données.'; break;
          case 422:  errorMessage = error.error?.message || 'Données invalides.'; break;
          case 500:  errorMessage = 'Erreur serveur interne.'; break;
          default:   errorMessage = error.error?.message || `Erreur ${error.status}`; break;
        }
      }

      notificationService.showError(errorMessage);
      return throwError(() => error);
    })
  );
};
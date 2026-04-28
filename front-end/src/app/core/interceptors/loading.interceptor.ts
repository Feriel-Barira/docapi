import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../services/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  // ✅ Ne pas afficher le loader pour les requêtes de données
  const skipUrls = [
    '/Dashboard', '/Processus', '/NonConformite', 
    '/Document', '/Indicateur', '/PointControle',
    '/Procedure', '/ActionCorrective', '/Users'
  ];

  const shouldSkip = skipUrls.some(url => req.url.includes(url));
  
  if (shouldSkip) {
    return next(req);  // ← Pas de loader
  }

  loadingService.show();
  return next(req).pipe(
    finalize(() => loadingService.hide())
  );
};
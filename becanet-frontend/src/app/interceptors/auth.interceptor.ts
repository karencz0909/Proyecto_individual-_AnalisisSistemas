import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { SesionService } from '../services/sesion.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const sesionService = inject(SesionService);
  const router = inject(Router);
  const token = sesionService.obtenerToken();

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        sesionService.cerrarSesion();
        router.navigate(['/login'], { queryParams: { sesionExpirada: 'true' } });
      }
      return throwError(() => error);
    })
  );
};
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Verifica si el usuario ha iniciado sesión
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

// Verifica si el usuario tiene el rol necesario para la ruta
export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const requiredRole = route.data['role'];

  const userRole = authService.getUserRole();

  if (userRole === requiredRole) {
    return true;
  }

  // Si no tiene el rol, redirige a su pantalla correspondiente o al login
  const defaultPath = userRole === 'ADMIN' ? '/solicitudes-admin' : '/mi-solicitud';
  return router.createUrlTree([defaultPath]);
};
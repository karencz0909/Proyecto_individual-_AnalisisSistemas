import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SesionService } from '../../services/sesion.service'; // Subir dos niveles (../../)

export const authGuard: CanActivateFn = (route, state) => {
  const sesionService = inject(SesionService);
  const router = inject(Router);

  if (!sesionService.estaAutenticado()) {
    router.navigate(['/login']);
    return false;
  }

  const rolesPermitidos = route.data?.['roles'] as Array<string>;
  const rolUsuario = sesionService.obtenerRol();

  if (rolesPermitidos && rolUsuario && !rolesPermitidos.includes(rolUsuario)) {
    router.navigate(['/']);
    return false;
  }

  return true;
};
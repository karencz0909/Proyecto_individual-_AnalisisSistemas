import { Routes } from '@angular/router';
import { authGuard } from './guards/auth/auth.guard'; // Ajustado a la carpeta /guards/auth/

export const routes: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) 
  },
  { 
    path: 'login', 
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) 
  },
  { 
    path: 'registro', 
    loadComponent: () => import('./features/registro/registro.component').then(m => m.RegistroComponent) 
  },
  { 
    path: 'mi-solicitud', 
    loadComponent: () => import('./pages/solicitudes/solicitudes.component').then(m => m.SolicitudesComponent),
    canActivate: [authGuard],
    data: { roles: ['ESTUDIANTE'] }
  },
  { 
    path: 'documentacion', 
    loadComponent: () => import('./features/documentos/documentos.component').then(m => m.DocumentosComponent),
    canActivate: [authGuard],
    data: { roles: ['ESTUDIANTE'] }
  },
  { 
    path: 'solicitudes-admin', 
    loadComponent: () => import('./pages/solicitudes-admin/solicitudes-admin.component').then(m => m.SolicitudesAdminComponent),
    canActivate: [authGuard],
    data: { roles: ['COORDINADOR'] }
  },
  { 
    path: 'comites', 
    loadComponent: () => import('./features/comites/comites.component').then(m => m.ComitesComponent),
    canActivate: [authGuard],
    data: { roles: ['COORDINADOR'] }
  },
  {
    path: 'usuarios',
    loadComponent: () => import('./pages/usuarios/usuarios.component').then(m => m.UsuariosComponent),
    canActivate: [authGuard],
    data: { roles: ['COORDINADOR'] }
  },
  {
    path: 'auditoria',
    loadComponent: () => import('./pages/auditoria/auditoria.component').then(m => m.AuditoriaComponent),
    canActivate: [authGuard],
    data: { roles: ['COORDINADOR'] }
  },
  { 
    path: 'evaluaciones', 
    loadComponent: () => import('./features/evaluaciones/evaluaciones.component').then(m => m.EvaluacionesComponent),
    canActivate: [authGuard],
    data: { roles: ['EVALUADOR'] }
  },
  { path: '**', redirectTo: '' }
];

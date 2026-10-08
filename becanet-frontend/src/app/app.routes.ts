import { Routes } from '@angular/router';
import { LoginComponent } from './features/login/login.component';
import { SolicitudesAdminComponent } from './pages/solicitudes-admin/solicitudes-admin.component';
import { MiSolicitudComponent } from './pages/mi-solicitud/mi-solicitud.component';
import { authGuard } from './guards/auth/auth.guard';

export const routes: Routes = [
  // Si entra a la raíz, redirige al login
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },

  { 
    path: 'solicitudes-admin', 
    component: SolicitudesAdminComponent, 
    canActivate: [authGuard] 
  },
  { 
    path: 'mi-solicitud', 
    component: MiSolicitudComponent, 
    canActivate: [authGuard] 
  },

  { path: '**', redirectTo: 'login' }
];
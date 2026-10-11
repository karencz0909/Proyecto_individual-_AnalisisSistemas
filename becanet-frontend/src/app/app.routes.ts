import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './features/login/login.component';
import { SolicitudesAdminComponent } from './pages/solicitudes-admin/solicitudes-admin.component';
import { MiSolicitudComponent } from './pages/mi-solicitud/mi-solicitud.component';
import { authGuard } from './guards/auth/auth.guard';

export const routes: Routes = [
  // Ruta raíz: Muestra la página informativa e interactiva de BecaNet
  { path: '', component: HomeComponent },

  // Ruta de acceso / login
  { path: 'login', component: LoginComponent },

  // Rutas protegidas
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

  // Redirección ante cualquier ruta inválida
  { path: '**', redirectTo: '' }
];
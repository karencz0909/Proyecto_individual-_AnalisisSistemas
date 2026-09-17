import { Routes } from '@angular/router';
import { SolicitudesComponent } from './features/solicitudes/solicitudes.component';
import { DocumentosComponent } from './features/documentos/documentos.component';
import { ComitesComponent } from './features/comites/comites.component';
import { EvaluacionesComponent } from './features/evaluaciones/evaluaciones.component';

export const routes: Routes = [
  { path: '', redirectTo: 'solicitudes', pathMatch: 'full' },
  { path: 'solicitudes', component: SolicitudesComponent, title: 'BecaNet | Solicitudes' },
  { path: 'documentos', component: DocumentosComponent, title: 'BecaNet | Documentos' },
  { path: 'comites', component: ComitesComponent, title: 'BecaNet | Comités' },
  { path: 'evaluaciones', component: EvaluacionesComponent, title: 'BecaNet | Evaluaciones' },
  { path: '**', redirectTo: 'solicitudes' }
];

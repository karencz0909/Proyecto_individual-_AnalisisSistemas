import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ComiteService } from '../../services/comite.service';
import { Comite } from '../../models/comite.model';

@Component({
  selector: 'app-comites',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comites.component.html',
  styleUrl: './comites.component.css'
})
export class ComitesComponent {
  // Formulario "Crear comité" (US-010)
  nombreComite = '';
  idsEvaluadoresTexto = ''; // el usuario los escribe separados por coma, ej: "3,5,8"

  // Formulario "Asignar solicitudes" (US-011)
  idComiteAsignar: number | null = null;
  idsSolicitudesTexto = '';

  comites: Comite[] = [];
  cargando = false;
  mensajeError = '';
  mensajeExito = '';

  constructor(private comiteService: ComiteService) {}

  ngOnInit(): void {
    this.cargarComites();
  }

  crearComite(): void {
    this.limpiarMensajes();

    const idsEvaluadores = this.parsearListaDeIds(this.idsEvaluadoresTexto);

    if (!this.nombreComite.trim()) {
      this.mensajeError = 'Debes indicar un nombre para el comité.';
      return;
    }
    if (idsEvaluadores.length < 2) {
      this.mensajeError = 'Un comité debe tener al menos 2 miembros (IDs de evaluadores).';
      return;
    }

    this.cargando = true;
    this.comiteService.crear({ nombre: this.nombreComite, idsEvaluadores }).subscribe({
      next: (creado) => {
        this.mensajeExito = `Comité "${creado.nombre}" creado con ${creado.miembros.length} miembros.`;
        this.nombreComite = '';
        this.idsEvaluadoresTexto = '';
        this.cargando = false;
        this.cargarComites();
      },
      error: (err) => {
        this.mensajeError = err?.error?.mensaje || 'No se pudo crear el comité.';
        this.cargando = false;
      }
    });
  }

  cargarComites(): void {
    this.comiteService.obtenerTodos().subscribe({
      next: (data) => (this.comites = data),
      error: () => (this.mensajeError = 'No se pudieron cargar los comités.')
    });
  }

  asignarSolicitudes(): void {
    this.limpiarMensajes();
    const idsSolicitudes = this.parsearListaDeIds(this.idsSolicitudesTexto);

    if (!this.idComiteAsignar) {
      this.mensajeError = 'Indica el ID del comité al que se asignarán las solicitudes.';
      return;
    }
    if (idsSolicitudes.length === 0) {
      this.mensajeError = 'Indica al menos un ID de solicitud a asignar.';
      return;
    }

    this.cargando = true;
    this.comiteService
      .asignarSolicitudes({ idComite: this.idComiteAsignar, idsSolicitudes })
      .subscribe({
        next: () => {
          this.mensajeExito = 'Solicitudes asignadas correctamente al comité.';
          this.idsSolicitudesTexto = '';
          this.cargando = false;
          this.cargarComites();
        },
        error: (err) => {
          this.mensajeError = err?.error?.mensaje || 'No se pudieron asignar las solicitudes.';
          this.cargando = false;
        }
      });
  }

  private parsearListaDeIds(texto: string): number[] {
    return texto
      .split(',')
      .map((t) => parseInt(t.trim(), 10))
      .filter((n) => !isNaN(n) && n > 0);
  }

  private limpiarMensajes(): void {
    this.mensajeError = '';
    this.mensajeExito = '';
  }
}

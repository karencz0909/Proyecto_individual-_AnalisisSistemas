import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SolicitudService } from '../../services/solicitud.service';
import { CrearSolicitud, Solicitud } from '../../models/solicitud.model';

@Component({
  selector: 'app-solicitudes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './solicitudes.component.html',
  styleUrl: './solicitudes.component.css'
})
export class SolicitudesComponent {
  // Campos del formulario "Crear solicitud" (US-007)
  nuevaSolicitud: CrearSolicitud = {
    idEstudiante: 0,
    idConvocatoria: 0,
    observaciones: ''
  };

  // Filtro para listar las solicitudes de un estudiante
  idEstudianteConsulta: number | null = null;

  solicitudes: Solicitud[] = [];
  cargando = false;
  mensajeError = '';
  mensajeExito = '';

  constructor(private solicitudService: SolicitudService) {}

  crearSolicitud(): void {
    this.limpiarMensajes();

    if (!this.nuevaSolicitud.idEstudiante || !this.nuevaSolicitud.idConvocatoria) {
      this.mensajeError = 'Debes indicar el ID del estudiante y el ID de la convocatoria.';
      return;
    }

    this.cargando = true;
    this.solicitudService.crear(this.nuevaSolicitud).subscribe({
      next: (creada) => {
        this.mensajeExito = `Solicitud #${creada.id} creada correctamente (estado: ${creada.estado}).`;
        this.nuevaSolicitud = { idEstudiante: 0, idConvocatoria: 0, observaciones: '' };
        this.cargando = false;
        // Si ya se estaba consultando este estudiante, se refresca la lista
        if (this.idEstudianteConsulta === creada.idEstudiante) {
          this.buscarPorEstudiante();
        }
      },
      error: (err) => {
        this.mensajeError = err?.error?.mensaje || 'No se pudo crear la solicitud.';
        this.cargando = false;
      }
    });
  }

  buscarPorEstudiante(): void {
    this.limpiarMensajes();
    if (!this.idEstudianteConsulta) {
      this.mensajeError = 'Ingresa el ID del estudiante para consultar sus solicitudes.';
      return;
    }

    this.cargando = true;
    this.solicitudService.obtenerPorEstudiante(this.idEstudianteConsulta).subscribe({
      next: (data) => {
        this.solicitudes = data;
        this.cargando = false;
      },
      error: () => {
        this.mensajeError = 'No se pudieron cargar las solicitudes.';
        this.cargando = false;
      }
    });
  }

  cancelarSolicitud(solicitud: Solicitud): void {
    const motivo = prompt('¿Motivo de la cancelación?') || undefined;
    this.solicitudService.cancelar(solicitud.id, { motivo }).subscribe({
      next: () => {
        this.mensajeExito = `Solicitud #${solicitud.id} cancelada.`;
        this.buscarPorEstudiante();
      },
      error: (err) => {
        this.mensajeError = err?.error?.mensaje || 'No se pudo cancelar la solicitud.';
      }
    });
  }

  claseBadge(estado: string): string {
    return 'badge badge-' + estado.toLowerCase().replace('_', '-');
  }

  private limpiarMensajes(): void {
    this.mensajeError = '';
    this.mensajeExito = '';
  }
}

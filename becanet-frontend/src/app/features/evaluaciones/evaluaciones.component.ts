import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EvaluacionService } from '../../services/evaluacion.service';
import { Evaluacion, RegistrarEvaluacion } from '../../models/evaluacion.model';

@Component({
  selector: 'app-evaluaciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './evaluaciones.component.html',
  styleUrl: './evaluaciones.component.css'
})
export class EvaluacionesComponent {
  nuevaEvaluacion: RegistrarEvaluacion = {
    idSolicitud: 0,
    idEvaluador: 0,
    puntaje: 0,
    observaciones: ''
  };

  idSolicitudConsulta: number | null = null;
  evaluaciones: Evaluacion[] = [];

  cargando = false;
  mensajeError = '';
  mensajeExito = '';

  constructor(private evaluacionService: EvaluacionService) {}

  registrarEvaluacion(): void {
    this.limpiarMensajes();

    if (!this.nuevaEvaluacion.idSolicitud || !this.nuevaEvaluacion.idEvaluador) {
      this.mensajeError = 'Debes indicar el ID de la solicitud y el ID del evaluador.';
      return;
    }
    if (this.nuevaEvaluacion.puntaje < 0 || this.nuevaEvaluacion.puntaje > 100) {
      this.mensajeError = 'El puntaje debe estar entre 0 y 100.';
      return;
    }
    if (!this.nuevaEvaluacion.observaciones.trim()) {
      this.mensajeError = 'Las observaciones son obligatorias para dejar constancia de tu decisión.';
      return;
    }

    this.cargando = true;
    this.evaluacionService.registrar(this.nuevaEvaluacion).subscribe({
      next: (creada) => {
        this.mensajeExito = `Evaluación registrada para la solicitud #${creada.idSolicitud}.`;
        this.nuevaEvaluacion = { idSolicitud: 0, idEvaluador: 0, puntaje: 0, observaciones: '' };
        this.cargando = false;
        if (this.idSolicitudConsulta === creada.idSolicitud) {
          this.consultarEvaluaciones();
        }
      },
      error: (err) => {
        this.mensajeError = err?.error?.mensaje || 'No se pudo registrar la evaluación.';
        this.cargando = false;
      }
    });
  }

  consultarEvaluaciones(): void {
    this.limpiarMensajes();
    if (!this.idSolicitudConsulta) {
      this.mensajeError = 'Indica el ID de la solicitud para ver sus evaluaciones.';
      return;
    }

    this.cargando = true;
    this.evaluacionService.obtenerPorSolicitud(this.idSolicitudConsulta).subscribe({
      next: (data) => {
        this.evaluaciones = data;
        this.cargando = false;
      },
      error: () => {
        this.mensajeError = 'No se pudieron cargar las evaluaciones.';
        this.cargando = false;
      }
    });
  }

  private limpiarMensajes(): void {
    this.mensajeError = '';
    this.mensajeExito = '';
  }
}

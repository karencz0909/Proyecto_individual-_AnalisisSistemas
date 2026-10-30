import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SolicitudService } from '../../services/solicitud.service';
import { ConvocatoriaService } from '../../services/convocatoria.service';
import { SesionService } from '../../services/sesion.service';
import { NotificacionSolicitud, Solicitud } from '../../models/solicitud.model';
import { Convocatoria } from '../../models/convocatoria.model';

interface Paso {
  titulo: string;
  detalle: string;
  estado: 'hecho' | 'actual' | 'pendiente' | 'error';
}

interface Aviso {
  tipo: 'success' | 'error' | 'info';
  texto: string;
}

const CLAVE_BORRADOR = 'becanet_borrador_solicitud';

@Component({
  selector: 'app-solicitudes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './solicitudes.component.html',
  styleUrls: ['../../shared/estilos-paginas.css', './solicitudes.component.scss']
})
export class SolicitudesComponent implements OnInit, OnDestroy {
  readonly MIN_MOTIVO = 20;

  convocatorias: Convocatoria[] = [];
  solicitudes: Solicitud[] = [];
  notificaciones: NotificacionSolicitud[] = [];

  convocatoriaElegida: number | null = null;
  motivo = '';
  montoSolicitado: number | null = null;

  cargando = true;
  enviando = false;
  aviso: Aviso | null = null;
  borradorGuardado = false;
  cancelandoId: number | null = null;
  ultimaActualizacion: Date | null = null;

  private temporizador: ReturnType<typeof setInterval> | null = null;

  constructor(
    private solicitudService: SolicitudService,
    private convocatoriaService: ConvocatoriaService,
    private sesionService: SesionService
  ) {}

  ngOnInit(): void {
    this.restaurarBorrador();
    this.cargarTodo();
    // El estado de la solicitud se actualiza solo cada 20 segundos
    this.temporizador = setInterval(() => this.cargarSolicitudes(), 20000);
  }

  ngOnDestroy(): void {
    if (this.temporizador) clearInterval(this.temporizador);
  }

  get nombre(): string {
    return this.sesionService.obtenerNombre().split(' ')[0];
  }

  // ------------------------------------------------------------------
  // Carga de datos
  // ------------------------------------------------------------------
  cargarTodo(): void {
    this.cargando = true;
    forkJoin({
      convocatorias: this.convocatoriaService.obtenerAbiertas(),
      solicitudes: this.solicitudService.obtenerMias(),
      notificaciones: this.solicitudService.obtenerNotificacionesMias()
    }).subscribe({
      next: (r) => {
        this.convocatorias = r.convocatorias;
        this.solicitudes = r.solicitudes;
        this.notificaciones = r.notificaciones;
        this.ultimaActualizacion = new Date();
        this.cargando = false;
      },
      error: (err) => {
        this.aviso = { tipo: 'error', texto: this.extraerError(err) };
        this.cargando = false;
      }
    });
  }

  cargarSolicitudes(): void {
    forkJoin({ solicitudes: this.solicitudService.obtenerMias(), notificaciones: this.solicitudService.obtenerNotificacionesMias() }).subscribe({
      next: (r) => {
        this.solicitudes = r.solicitudes;
        this.notificaciones = r.notificaciones;
        this.ultimaActualizacion = new Date();
      },
      error: () => { /* si falla el refresco automático no molestamos al usuario */ }
    });
  }

  // ------------------------------------------------------------------
  // Nueva solicitud
  // ------------------------------------------------------------------
  yaPostulo(idConvocatoria: number): boolean {
    return this.solicitudes.some(s => s.idConvocatoria === idConvocatoria && s.estado !== 'CANCELADA');
  }

  get convocatoriasDisponibles(): Convocatoria[] {
    return this.convocatorias.filter(c => !this.yaPostulo(c.id));
  }

  elegir(c: Convocatoria): void {
    if (this.yaPostulo(c.id)) return;
    this.convocatoriaElegida = c.id;
    this.guardarBorrador();
  }

  get motivoValido(): boolean {
    return this.motivo.trim().length >= this.MIN_MOTIVO;
  }

  get puedeEnviar(): boolean {
    return this.convocatoriaElegida !== null && this.motivoValido && this.montoSolicitado !== null && this.montoSolicitado > 0 && !this.enviando;
  }

  enviar(): void {
    if (!this.puedeEnviar || this.convocatoriaElegida === null) return;

    this.enviando = true;
    this.aviso = null;

    this.solicitudService
      .crearMia({ idConvocatoria: this.convocatoriaElegida, motivo: this.motivo.trim(), montoSolicitado: this.montoSolicitado! })
      .subscribe({
        next: () => {
          this.enviando = false;
          this.limpiarFormulario();
          this.aviso = {
            tipo: 'success',
            texto: '¡Solicitud enviada! Ahora sube tu certificación de estudios para que pueda ser revisada.'
          };
          this.cargarSolicitudes();
        },
        error: (err) => {
          this.enviando = false;
          // El borrador se conserva para que pueda reintentar sin perder lo escrito
          this.aviso = { tipo: 'error', texto: this.extraerError(err) };
        }
      });
  }

  limpiarFormulario(): void {
    this.convocatoriaElegida = null;
    this.motivo = '';
    this.montoSolicitado = null;
    this.borradorGuardado = false;
    localStorage.removeItem(CLAVE_BORRADOR);
  }

  // ------------------------------------------------------------------
  // Borrador automático (no se pierde lo escrito si falla algo o expira la sesión)
  // ------------------------------------------------------------------
  guardarBorrador(): void {
    if (!this.motivo && this.convocatoriaElegida === null) {
      localStorage.removeItem(CLAVE_BORRADOR);
      this.borradorGuardado = false;
      return;
    }
    localStorage.setItem(
      CLAVE_BORRADOR,
      JSON.stringify({ idConvocatoria: this.convocatoriaElegida, motivo: this.motivo, montoSolicitado: this.montoSolicitado })
    );
    this.borradorGuardado = true;
  }

  private restaurarBorrador(): void {
    try {
      const guardado = localStorage.getItem(CLAVE_BORRADOR);
      if (!guardado) return;
      const b = JSON.parse(guardado);
      this.convocatoriaElegida = b.idConvocatoria ?? null;
      this.motivo = b.motivo ?? '';
      this.montoSolicitado = b.montoSolicitado ?? null;
      this.borradorGuardado = true;
      this.aviso = { tipo: 'info', texto: 'Recuperamos el borrador que dejaste a medias.' };
    } catch {
      localStorage.removeItem(CLAVE_BORRADOR);
    }
  }

  // ------------------------------------------------------------------
  // Cancelar (con confirmación, para evitar errores)
  // ------------------------------------------------------------------
  puedeCancelar(s: Solicitud): boolean {
    return s.estado === 'EN_PROCESO';
  }

  pedirCancelar(id: number): void { this.cancelandoId = id; }
  descartarCancelar(): void { this.cancelandoId = null; }

  confirmarCancelar(s: Solicitud): void {
    this.solicitudService.cancelar(s.id, { motivo: 'Cancelada por el estudiante' }).subscribe({
      next: () => {
        this.cancelandoId = null;
        this.aviso = { tipo: 'info', texto: 'Tu solicitud fue cancelada. Puedes volver a postular cuando quieras.' };
        this.cargarSolicitudes();
      },
      error: (err) => {
        this.cancelandoId = null;
        this.aviso = { tipo: 'error', texto: this.extraerError(err) };
      }
    });
  }

  // ------------------------------------------------------------------
  // Presentación del estado
  // ------------------------------------------------------------------
  etiqueta(s: Solicitud): { texto: string; clase: string } {
    switch (s.estado) {
      case 'CANCELADA': return { texto: 'Cancelada', clase: 'badge-gray' };
      case 'APROBADA':  return { texto: 'Aprobada', clase: 'badge-green' };
      case 'RECHAZADA': return { texto: 'Rechazada', clase: 'badge-red' };
      case 'EVALUADA':
        return s.montoSolicitado > 10_000 && s.aprobaciones?.some(a => a.tipo === 'COORD_PRE' && a.aprobada)
          ? { texto: 'Preaprobada', clase: 'badge-purple' }
          : { texto: 'Evaluada', clase: 'badge-purple' };
    }
    if (!s.tieneCertificacionEstudios) return { texto: 'Falta documentación', clase: 'badge-yellow' };
    if (s.estadoRevision === 'EN_REVISION') return { texto: 'En revisión', clase: 'badge-blue' };
    return { texto: 'Recibida', clase: 'badge-blue' };
  }

  pasos(s: Solicitud): Paso[] {
    const evaluada = s.estado === 'EVALUADA' || s.estado === 'APROBADA' || s.estado === 'RECHAZADA';
    const revision = s.estadoRevision ?? 'SIN_ASIGNAR';

    let detalleRevision = 'Un evaluador está revisando tu solicitud';
    if (!s.tieneCertificacionEstudios) detalleRevision = 'Sube tu certificación para continuar';
    else if (revision === 'SIN_ASIGNAR') detalleRevision = 'Esperando asignación de evaluador';
    if (evaluada) detalleRevision = 'Revisión completada';

    const resultado: Paso =
      s.estado === 'APROBADA'  ? { titulo: 'Aprobada',  detalle: '¡Felicidades!', estado: 'hecho' } :
      s.estado === 'RECHAZADA' ? { titulo: 'Rechazada', detalle: 'Revisa el motivo', estado: 'error' } :
      s.estado === 'EVALUADA'  ? { titulo: 'Resultado', detalle: 'Decisión de coordinación', estado: 'actual' } :
                                 { titulo: 'Resultado', detalle: 'Pendiente', estado: 'pendiente' };

    if (s.montoSolicitado > 10_000 && resultado.estado === 'actual') {
      const preaprobacion = s.aprobaciones?.find(a => a.tipo === 'COORD_PRE');
      resultado.detalle = preaprobacion?.aprobada
          ? 'Preaprobada; pendiente de aprobación final de otro coordinador'
          : 'Evaluación completada; pendiente de preaprobación de coordinación';
    }

    return [
      { titulo: 'Enviada', detalle: new Date(s.fechaCreacion).toLocaleDateString('es-GT'), estado: 'hecho' },
      { titulo: 'En revisión', detalle: detalleRevision, estado: evaluada ? 'hecho' : 'actual' },
      { titulo: 'Evaluada', detalle: evaluada ? 'Tu solicitud fue calificada' : 'Pendiente', estado: evaluada ? 'hecho' : 'pendiente' },
      resultado
    ];
  }

  diasRestantes(c: Convocatoria): number {
    return Math.ceil((new Date(c.fechaCierre).getTime() - Date.now()) / 86400000);
  }

  private extraerError(err: any): string {
    if (err?.status === 0) return 'No se pudo conectar con el servidor. Verifica tu conexión e inténtalo de nuevo.';
    if (err?.error?.mensaje) return err.error.mensaje;
    const errores = err?.error?.errors;
    if (errores) {
      const primero = Object.values(errores)[0] as string[];
      if (primero?.length) return primero[0];
    }
    return 'Ocurrió un error inesperado. Inténtalo de nuevo.';
  }
}

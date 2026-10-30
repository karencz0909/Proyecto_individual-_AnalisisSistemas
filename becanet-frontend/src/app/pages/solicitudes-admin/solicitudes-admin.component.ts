import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AsignacionService } from '../../services/asignacion.service';
import { SolicitudService } from '../../services/solicitud.service';
import { ComiteService } from '../../services/comite.service';
import { DocumentoService } from '../../services/documento.service';
import {
  DocumentoResumen,
  EvaluadorOpcion,
  ResultadoDistribucion,
  SolicitudCoordinador
} from '../../models/asignacion.model';
import { Comite } from '../../models/comite.model';
import { SesionService } from '../../services/sesion.service';

type Etapa = 'SIN_ASIGNAR' | 'EN_REVISION' | 'EVALUADA' | 'APROBADA' | 'RECHAZADA' | 'CANCELADA';
type Filtro = 'TODAS' | Etapa;

interface Aviso {
  tipo: 'success' | 'error' | 'info';
  texto: string;
}

@Component({
  selector: 'app-solicitudes-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './solicitudes-admin.component.html',
  styleUrls: ['../../shared/estilos-paginas.css', './solicitudes-admin.component.css']
})
export class SolicitudesAdminComponent implements OnInit {
  solicitudes: SolicitudCoordinador[] = [];
  evaluadores: EvaluadorOpcion[] = [];
  comites: Comite[] = [];

  filtro: Filtro = 'TODAS';
  busqueda = '';
  expandida: number | null = null;

  comiteElegido: number | null = null;
  confirmandoDistribuir = false;
  distribuyendo = false;
  resultado: ResultadoDistribucion | null = null;

  confirmandoAprobar: number | null = null;
  rechazando: number | null = null;
  motivoRechazo = '';
  procesando: number | null = null;

  cargando = true;
  aviso: Aviso | null = null;

  readonly filtros: { id: Filtro; texto: string }[] = [
    { id: 'TODAS', texto: 'Todas' },
    { id: 'SIN_ASIGNAR', texto: 'Sin asignar' },
    { id: 'EN_REVISION', texto: 'En revisión' },
    { id: 'EVALUADA', texto: 'Por decidir' },
    { id: 'APROBADA', texto: 'Aprobadas' },
    { id: 'RECHAZADA', texto: 'Rechazadas' },
    { id: 'CANCELADA', texto: 'Canceladas' }
  ];

  constructor(
    private asignacionService: AsignacionService,
    private solicitudService: SolicitudService,
    private comiteService: ComiteService,
    private documentoService: DocumentoService,
    private sesionService: SesionService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  // ------------------------------------------------------------------
  // Datos
  // ------------------------------------------------------------------
  cargar(): void {
    this.cargando = true;
    forkJoin({
      solicitudes: this.asignacionService.obtenerSolicitudes(),
      evaluadores: this.asignacionService.obtenerEvaluadores(),
      comites: this.comiteService.obtenerTodos()
    }).subscribe({
      next: (r) => {
        this.solicitudes = r.solicitudes;
        this.evaluadores = r.evaluadores;
        this.comites = r.comites;
        this.cargando = false;
      },
      error: (err) => {
        this.cargando = false;
        this.aviso = { tipo: 'error', texto: this.extraerError(err) };
      }
    });
  }

  private recargarListas(): void {
    forkJoin({
      solicitudes: this.asignacionService.obtenerSolicitudes(),
      evaluadores: this.asignacionService.obtenerEvaluadores()
    }).subscribe({
      next: (r) => {
        this.solicitudes = r.solicitudes;
        this.evaluadores = r.evaluadores;
      },
      error: (err) => (this.aviso = { tipo: 'error', texto: this.extraerError(err) })
    });
  }

  // ------------------------------------------------------------------
  // Clasificación y filtros
  // ------------------------------------------------------------------
  etapa(s: SolicitudCoordinador): Etapa {
    if (s.estado === 'CANCELADA') return 'CANCELADA';
    if (s.estado === 'APROBADA') return 'APROBADA';
    if (s.estado === 'RECHAZADA') return 'RECHAZADA';
    if (s.estado === 'EVALUADA') return 'EVALUADA';
    return s.estadoRevision === 'EN_REVISION' ? 'EN_REVISION' : 'SIN_ASIGNAR';
  }

  etiqueta(s: SolicitudCoordinador): { texto: string; clase: string } {
    switch (this.etapa(s)) {
      case 'CANCELADA': return { texto: 'Cancelada', clase: 'badge-gray' };
      case 'APROBADA':  return { texto: 'Aprobada', clase: 'badge-green' };
      case 'RECHAZADA': return { texto: 'Rechazada', clase: 'badge-red' };
      case 'EVALUADA':
        return s.montoSolicitado > 10_000 && s.preaprobadaPorCoordinador
          ? { texto: 'Preaprobada', clase: 'badge-purple' }
          : { texto: 'Evaluada · por decidir', clase: 'badge-purple' };
      case 'EN_REVISION': return { texto: 'En revisión', clase: 'badge-blue' };
      default:
        return s.tieneCertificacionEstudios
          ? { texto: 'Sin asignar', clase: 'badge-yellow' }
          : { texto: 'Falta certificación', clase: 'badge-gray' };
    }
  }

  conteo(f: Filtro): number {
    return f === 'TODAS'
      ? this.solicitudes.length
      : this.solicitudes.filter(s => this.etapa(s) === f).length;
  }

  get filtradas(): SolicitudCoordinador[] {
    const texto = this.busqueda.trim().toLowerCase();
    return this.solicitudes.filter(s => {
      if (this.filtro !== 'TODAS' && this.etapa(s) !== this.filtro) return false;
      if (!texto) return true;
      return [s.nombreEstudiante, s.correoEstudiante, s.tituloConvocatoria, s.correoEvaluador ?? '', s.nombreEvaluador ?? '']
        .some(v => v.toLowerCase().includes(texto));
    });
  }

  get pendientesDeReparto(): number {
    return this.solicitudes.filter(s => this.etapa(s) === 'SIN_ASIGNAR' && s.tieneCertificacionEstudios).length;
  }

  get sinCertificacion(): number {
    return this.solicitudes.filter(s => this.etapa(s) === 'SIN_ASIGNAR' && !s.tieneCertificacionEstudios).length;
  }

  get cargaMaxima(): number {
    return Math.max(1, ...this.evaluadores.map(e => e.asignadas));
  }

  seleccionarFiltro(f: Filtro): void {
    this.filtro = f;
    this.expandida = null;
  }

  alternarDetalle(id: number): void {
    this.expandida = this.expandida === id ? null : id;
  }

  urlDe(d: DocumentoResumen): string {
    return this.documentoService.urlCompleta(d.urlArchivo);
  }

  // ------------------------------------------------------------------
  // Reparto automático
  // ------------------------------------------------------------------
  pedirDistribuir(): void {
    this.resultado = null;
    this.confirmandoDistribuir = true;
  }

  cancelarDistribuir(): void {
    this.confirmandoDistribuir = false;
  }

  confirmarDistribuir(): void {
    this.distribuyendo = true;
    this.asignacionService.distribuir(this.comiteElegido).subscribe({
      next: (r) => {
        this.distribuyendo = false;
        this.confirmandoDistribuir = false;
        this.resultado = r;
        this.aviso = {
          tipo: r.totalAsignadas > 0 ? 'success' : 'info',
          texto: r.totalAsignadas > 0
            ? `Se asignaron ${r.totalAsignadas} solicitud(es) de forma equilibrada entre los evaluadores.`
            : 'No había solicitudes pendientes con certificación de estudios para repartir.'
        };
        this.recargarListas();
      },
      error: (err) => {
        this.distribuyendo = false;
        this.confirmandoDistribuir = false;
        this.aviso = { tipo: 'error', texto: this.extraerError(err) };
      }
    });
  }

  // ------------------------------------------------------------------
  // Asignación manual
  // ------------------------------------------------------------------
  puedeReasignar(s: SolicitudCoordinador): boolean {
    return s.estado === 'EN_PROCESO' && s.estadoRevision !== 'REVISADO' && s.tieneCertificacionEstudios;
  }

  cambiarEvaluador(s: SolicitudCoordinador, valor: string): void {
    const idEvaluador = Number(valor);
    if (!idEvaluador || idEvaluador === s.idEvaluadorAsignado) return;

    this.procesando = s.idSolicitud;
    this.asignacionService.asignar(s.idSolicitud, idEvaluador).subscribe({
      next: (actualizada) => {
        this.procesando = null;
        this.solicitudes = this.solicitudes.map(x => x.idSolicitud === actualizada.idSolicitud ? actualizada : x);
        this.aviso = { tipo: 'success', texto: `Solicitud #${s.idSolicitud} asignada a ${actualizada.nombreEvaluador}.` };
        this.recargarListas();
      },
      error: (err) => {
        this.procesando = null;
        this.aviso = { tipo: 'error', texto: this.extraerError(err) };
        this.recargarListas(); // devuelve el selector al valor real
      }
    });
  }

  // ------------------------------------------------------------------
  // Aprobar / rechazar (con confirmación)
  // ------------------------------------------------------------------
  pedirAprobar(id: number): void {
    this.rechazando = null;
    this.confirmandoAprobar = id;
  }

  confirmarAprobar(s: SolicitudCoordinador): void {
    this.resolver(s, true);
  }

  esPreaprobadorActual(s: SolicitudCoordinador): boolean {
    return s.preaprobadaPorCoordinador
      && s.idPreaprobadorCoordinador != null
      && Number(this.sesionService.obtenerId()) === s.idPreaprobadorCoordinador;
  }

  pedirRechazo(id: number): void {
    this.confirmandoAprobar = null;
    this.motivoRechazo = '';
    this.rechazando = id;
  }

  cancelarAccion(): void {
    this.confirmandoAprobar = null;
    this.rechazando = null;
    this.motivoRechazo = '';
  }

  confirmarRechazo(s: SolicitudCoordinador): void {
    if (this.motivoRechazo.trim().length < 5) return;
    this.resolver(s, false);
  }

  private resolver(s: SolicitudCoordinador, aprobar: boolean): void {
    this.procesando = s.idSolicitud;
    this.solicitudService
      .resolver(s.idSolicitud, { aprobar, motivo: aprobar ? undefined : this.motivoRechazo.trim() })
      .subscribe({
        next: () => {
          this.procesando = null;
          this.cancelarAccion();
          const esMontoAlto = s.montoSolicitado > 10000;
          const textoResultado = aprobar && esMontoAlto && !s.preaprobadaPorCoordinador
            ? `Solicitud #${s.idSolicitud} preaprobada. Falta que otro coordinador registre la aprobación final.`
            : `Solicitud #${s.idSolicitud} ${aprobar ? 'aprobada' : 'rechazada'}. El estudiante verá el resultado de inmediato.`;
          this.aviso = {
            tipo: 'success',
            texto: textoResultado
          };
          this.recargarListas();
        },
        error: (err) => {
          this.procesando = null;
          this.aviso = { tipo: 'error', texto: this.extraerError(err) };
        }
      });
  }

  private extraerError(err: any): string {
    if (err?.status === 0) return 'No se pudo conectar con el servidor. Inténtalo de nuevo.';
    if (err?.error?.mensaje) return err.error.mensaje;
    const errores = err?.error?.errors;
    if (errores) {
      const primero = Object.values(errores)[0] as string[];
      if (primero?.length) return primero[0];
    }
    return 'Ocurrió un error inesperado. Inténtalo de nuevo.';
  }
}

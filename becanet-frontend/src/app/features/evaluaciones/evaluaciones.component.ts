import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AsignacionService } from '../../services/asignacion.service';
import { DocumentoService } from '../../services/documento.service';
import { SesionService } from '../../services/sesion.service';
import { AsignacionEvaluador, DocumentoResumen } from '../../models/asignacion.model';

type Columna = 'EN_REVISION' | 'REVISADO';

interface Tarjeta extends AsignacionEvaluador {
  puntajeEdit: number | null;
  obsEdit: string;
  guardando: boolean;
  error: string;
  sacudir: boolean;
  destacada: boolean;
}

interface Aviso {
  tipo: 'success' | 'error' | 'info';
  texto: string;
}

@Component({
  selector: 'app-evaluaciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './evaluaciones.component.html',
  styleUrls: ['../../shared/estilos-paginas.css', './evaluaciones.component.css']
})
export class EvaluacionesComponent implements OnInit {
  tarjetas: Tarjeta[] = [];
  cargando = true;
  aviso: Aviso | null = null;

  arrastrandoId: number | null = null;
  columnaSobre: Columna | null = null;

  constructor(
    private asignacionService: AsignacionService,
    private documentoService: DocumentoService,
    private sesionService: SesionService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  get nombre(): string {
    return this.sesionService.obtenerNombre().split(' ')[0];
  }

  // ------------------------------------------------------------------
  // Datos
  // ------------------------------------------------------------------
  cargar(): void {
    this.cargando = true;
    this.asignacionService.obtenerMias().subscribe({
      next: (lista) => {
        this.tarjetas = lista.map(a => this.aTarjeta(a));
        this.cargando = false;
      },
      error: (err) => {
        this.cargando = false;
        this.aviso = { tipo: 'error', texto: this.extraerError(err) };
      }
    });
  }

  private aTarjeta(a: AsignacionEvaluador): Tarjeta {
    let puntajeEdit: number | null = a.puntaje ?? null;
    let obsEdit = a.observacionesEvaluacion ?? '';

    // Si dejó una calificación a medias, se recupera (no se pierde nada al recargar)
    if (a.puntaje == null && !a.observacionesEvaluacion) {
      const borrador = this.leerBorrador(a.idSolicitud);
      if (borrador) {
        puntajeEdit = borrador.p;
        obsEdit = borrador.o;
      }
    }
    return { ...a, puntajeEdit, obsEdit, guardando: false, error: '', sacudir: false, destacada: false };
  }

  get enRevision(): Tarjeta[] {
    return this.tarjetas.filter(t => t.estadoRevision !== 'REVISADO');
  }

  get revisadas(): Tarjeta[] {
    return this.tarjetas.filter(t => t.estadoRevision === 'REVISADO');
  }

  get porcentaje(): number {
    return this.tarjetas.length
      ? Math.round((this.revisadas.length / this.tarjetas.length) * 100)
      : 0;
  }

  resuelta(t: Tarjeta): boolean {
    return t.estadoSolicitud === 'APROBADA' || t.estadoSolicitud === 'RECHAZADA';
  }

  trackPorId(_: number, t: Tarjeta): number {
    return t.idSolicitud;
  }

  urlDe(d: DocumentoResumen): string {
    return this.documentoService.urlCompleta(d.urlArchivo);
  }

  etiquetaDocumento(d: DocumentoResumen): string {
    return d.tipoDocumento === 'CERTIFICACION_ESTUDIOS' ? 'Certificación de estudios' : d.nombreArchivo;
  }

  // ------------------------------------------------------------------
  // Arrastrar y soltar entre columnas
  // ------------------------------------------------------------------
  iniciarArrastre(ev: DragEvent, t: Tarjeta): void {
    if (this.resuelta(t) || t.guardando) {
      ev.preventDefault();
      return;
    }
    this.arrastrandoId = t.idSolicitud;
    if (ev.dataTransfer) {
      ev.dataTransfer.setData('text/plain', String(t.idSolicitud));
      ev.dataTransfer.effectAllowed = 'move';
      const tarjeta = (ev.target as HTMLElement).closest('.kcard') as HTMLElement | null;
      if (tarjeta) ev.dataTransfer.setDragImage(tarjeta, 24, 24);
    }
  }

  finArrastre(): void {
    this.arrastrandoId = null;
    this.columnaSobre = null;
  }

  sobreColumna(ev: DragEvent, columna: Columna): void {
    ev.preventDefault(); // necesario para permitir soltar
    if (ev.dataTransfer) ev.dataTransfer.dropEffect = 'move';
    this.columnaSobre = columna;
  }

  salirColumna(columna: Columna): void {
    if (this.columnaSobre === columna) this.columnaSobre = null;
  }

  soltar(ev: DragEvent, columna: Columna): void {
    ev.preventDefault();
    const id = Number(ev.dataTransfer?.getData('text/plain')) || this.arrastrandoId;
    const tarjeta = this.tarjetas.find(t => t.idSolicitud === id);
    this.finArrastre();
    if (tarjeta) this.moverA(tarjeta, columna);
  }

  /** También lo usan los botones, para quien no pueda arrastrar. */
  moverA(t: Tarjeta, columna: Columna): void {
    if (columna === 'REVISADO' && t.estadoRevision !== 'REVISADO') {
      this.marcarRevisada(t);
    } else if (columna === 'EN_REVISION' && t.estadoRevision === 'REVISADO') {
      this.volverARevision(t);
    }
  }

  // ------------------------------------------------------------------
  // Acciones sobre una tarjeta
  // ------------------------------------------------------------------
  marcarRevisada(t: Tarjeta): void {
    // Prevención de errores: no se puede revisar sin puntaje ni observaciones
    if (t.puntajeEdit === null || t.puntajeEdit === undefined || t.puntajeEdit < 0 || t.puntajeEdit > 100) {
      this.rechazarMovimiento(t, 'Indica un puntaje entre 0 y 100 antes de marcarla como revisada.');
      return;
    }
    if (t.obsEdit.trim().length < 5) {
      this.rechazarMovimiento(t, 'Escribe tus observaciones (mínimo 5 caracteres) antes de marcarla como revisada.');
      return;
    }

    t.guardando = true;
    t.error = '';
    this.asignacionService
      .revisar(t.idSolicitud, { puntaje: t.puntajeEdit, observaciones: t.obsEdit.trim() })
      .subscribe({
        next: (a) => {
          this.aplicar(t, a);
          this.borrarBorrador(t.idSolicitud);
          this.destacar(t);
          this.aviso = { tipo: 'success', texto: `Revisión de ${t.nombreEstudiante} guardada. La solicitud pasó a "Revisado".` };
        },
        error: (err) => {
          t.guardando = false;
          t.error = this.extraerError(err);
        }
      });
  }

  volverARevision(t: Tarjeta): void {
    t.guardando = true;
    t.error = '';
    this.asignacionService.reabrir(t.idSolicitud).subscribe({
      next: (a) => {
        this.aplicar(t, a);
        this.destacar(t);
        this.aviso = { tipo: 'info', texto: `La solicitud de ${t.nombreEstudiante} volvió a "En revisión".` };
      },
      error: (err) => {
        t.guardando = false;
        t.error = this.extraerError(err);
      }
    });
  }

  private rechazarMovimiento(t: Tarjeta, mensaje: string): void {
    t.error = mensaje;
    t.sacudir = true;
    setTimeout(() => (t.sacudir = false), 500);
  }

  private aplicar(t: Tarjeta, a: AsignacionEvaluador): void {
    Object.assign(t, a, {
      puntajeEdit: a.puntaje ?? t.puntajeEdit,
      obsEdit: a.observacionesEvaluacion ?? t.obsEdit,
      guardando: false,
      error: ''
    });
  }

  private destacar(t: Tarjeta): void {
    t.destacada = true;
    setTimeout(() => (t.destacada = false), 1400);
  }

  // ------------------------------------------------------------------
  // Borrador automático de la calificación
  // ------------------------------------------------------------------
  guardarBorrador(t: Tarjeta): void {
    t.error = '';
    localStorage.setItem(
      this.claveBorrador(t.idSolicitud),
      JSON.stringify({ p: t.puntajeEdit, o: t.obsEdit })
    );
  }

  private leerBorrador(id: number): { p: number | null; o: string } | null {
    try {
      const guardado = localStorage.getItem(this.claveBorrador(id));
      return guardado ? JSON.parse(guardado) : null;
    } catch {
      return null;
    }
  }

  private borrarBorrador(id: number): void {
    localStorage.removeItem(this.claveBorrador(id));
  }

  private claveBorrador(id: number): string {
    return `becanet_eval_borrador_${id}`;
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

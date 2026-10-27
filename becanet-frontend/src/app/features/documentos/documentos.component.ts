import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DocumentoService } from '../../services/documento.service';
import { SolicitudService } from '../../services/solicitud.service';
import { Documento } from '../../models/documento.model';
import { Solicitud } from '../../models/solicitud.model';

type TipoRequisito = 'CERTIFICACION_ESTUDIOS' | 'OTRO';

interface Aviso {
  tipo: 'success' | 'error' | 'info';
  texto: string;
}

@Component({
  selector: 'app-documentos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './documentos.component.html',
  styleUrls: ['../../shared/estilos-paginas.css', './documentos.component.css']
})
export class DocumentosComponent implements OnInit {
  readonly CERT: TipoRequisito = 'CERTIFICACION_ESTUDIOS';
  readonly OTRO: TipoRequisito = 'OTRO';

  private readonly extensionesPermitidas = ['pdf', 'jpg', 'jpeg', 'png'];
  private readonly tamanoMaximoBytes = 5 * 1024 * 1024;

  solicitudes: Solicitud[] = [];
  idSeleccionada: number | null = null;
  documentos: Documento[] = [];

  // Estado por cada espacio de carga (certificación / otros)
  archivos: Record<string, File | null> = { CERTIFICACION_ESTUDIOS: null, OTRO: null };
  arrastrando: Record<string, boolean> = { CERTIFICACION_ESTUDIOS: false, OTRO: false };
  subiendo: Record<string, boolean> = { CERTIFICACION_ESTUDIOS: false, OTRO: false };
  errores: Record<string, string> = { CERTIFICACION_ESTUDIOS: '', OTRO: '' };

  cargando = true;
  aviso: Aviso | null = null;

  constructor(
    private documentoService: DocumentoService,
    private solicitudService: SolicitudService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.cargarSolicitudes(true);
  }

  // ------------------------------------------------------------------
  // Solicitudes y documentos
  // ------------------------------------------------------------------
  private cargarSolicitudes(primeraVez: boolean): void {
    this.solicitudService.obtenerMias().subscribe({
      next: (lista) => {
        // Las canceladas ya no necesitan documentos
        this.solicitudes = lista.filter(s => s.estado !== 'CANCELADA');
        this.cargando = false;

        if (primeraVez) {
          const deUrl = Number(this.route.snapshot.queryParamMap.get('solicitud'));
          const existeEnUrl = this.solicitudes.some(s => s.id === deUrl);
          this.idSeleccionada = existeEnUrl ? deUrl : (this.solicitudes[0]?.id ?? null);
          this.cargarDocumentos();
        }
      },
      error: (err) => {
        this.cargando = false;
        this.aviso = { tipo: 'error', texto: this.extraerError(err) };
      }
    });
  }

  alCambiarSolicitud(): void {
    this.aviso = null;
    this.archivos = { CERTIFICACION_ESTUDIOS: null, OTRO: null };
    this.errores = { CERTIFICACION_ESTUDIOS: '', OTRO: '' };
    this.cargarDocumentos();
  }

  private cargarDocumentos(): void {
    if (this.idSeleccionada === null) { this.documentos = []; return; }
    this.documentoService.obtenerPorSolicitud(this.idSeleccionada).subscribe({
      next: (docs) => (this.documentos = docs),
      error: (err) => (this.aviso = { tipo: 'error', texto: this.extraerError(err) })
    });
  }

  get solicitudActual(): Solicitud | undefined {
    return this.solicitudes.find(s => s.id === this.idSeleccionada);
  }

  get certificaciones(): Documento[] {
    return this.documentos.filter(d => d.tipoDocumento === this.CERT);
  }

  get otros(): Documento[] {
    return this.documentos.filter(d => d.tipoDocumento !== this.CERT);
  }

  get tieneCertificacion(): boolean {
    return this.certificaciones.length > 0;
  }

  urlDe(d: Documento): string {
    return this.documentoService.urlCompleta(d.urlArchivo);
  }

  // ------------------------------------------------------------------
  // Selección del archivo (botón o arrastrar y soltar)
  // ------------------------------------------------------------------
  alElegirArchivo(tipo: TipoRequisito, evento: Event): void {
    const input = evento.target as HTMLInputElement;
    this.preparar(tipo, input.files?.[0] ?? null);
    input.value = ''; // permite volver a elegir el mismo archivo
  }

  alArrastrarSobre(tipo: TipoRequisito, evento: DragEvent): void {
    evento.preventDefault();
    this.arrastrando[tipo] = true;
  }

  alSalirDeZona(tipo: TipoRequisito): void {
    this.arrastrando[tipo] = false;
  }

  alSoltar(tipo: TipoRequisito, evento: DragEvent): void {
    evento.preventDefault();
    this.arrastrando[tipo] = false;
    this.preparar(tipo, evento.dataTransfer?.files?.[0] ?? null);
  }

  private preparar(tipo: TipoRequisito, archivo: File | null): void {
    this.errores[tipo] = '';
    this.aviso = null;
    if (!archivo) return;

    // Prevención de errores: se valida ANTES de subir
    const extension = archivo.name.split('.').pop()?.toLowerCase() ?? '';
    if (!this.extensionesPermitidas.includes(extension)) {
      this.errores[tipo] = 'Formato no permitido. Solo se aceptan PDF, JPG o PNG.';
      this.archivos[tipo] = null;
      return;
    }
    if (archivo.size > this.tamanoMaximoBytes) {
      this.errores[tipo] = `El archivo pesa ${this.formatoTamano(archivo.size)} y el máximo es 5 MB.`;
      this.archivos[tipo] = null;
      return;
    }
    this.archivos[tipo] = archivo;
  }

  quitar(tipo: TipoRequisito): void {
    this.archivos[tipo] = null;
    this.errores[tipo] = '';
  }

  subir(tipo: TipoRequisito): void {
    const archivo = this.archivos[tipo];
    if (!archivo || this.idSeleccionada === null) return;

    this.subiendo[tipo] = true;
    this.aviso = null;

    this.documentoService.cargar(this.idSeleccionada, archivo, tipo).subscribe({
      next: () => {
        this.subiendo[tipo] = false;
        this.archivos[tipo] = null;
        this.aviso = {
          tipo: 'success',
          texto: tipo === this.CERT
            ? '¡Certificación de estudios recibida! Tu solicitud ya puede ser revisada.'
            : 'Documento cargado correctamente.'
        };
        this.cargarDocumentos();
        this.cargarSolicitudes(false);
      },
      error: (err) => {
        this.subiendo[tipo] = false;
        // El archivo se conserva para poder reintentar sin volver a elegirlo
        this.errores[tipo] = this.extraerError(err);
      }
    });
  }

  formatoTamano(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  private extraerError(err: any): string {
    if (err?.status === 0) return 'No se pudo conectar con el servidor. Inténtalo de nuevo.';
    if (err?.status === 403) return 'No tienes permiso para modificar esta solicitud.';
    if (err?.error?.mensaje) return err.error.mensaje;
    return 'Ocurrió un error inesperado. Inténtalo de nuevo.';
  }
}

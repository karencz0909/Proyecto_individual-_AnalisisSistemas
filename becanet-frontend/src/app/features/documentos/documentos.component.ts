import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentoService } from '../../services/documento.service';
import { Documento } from '../../models/documento.model';

@Component({
  selector: 'app-documentos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documentos.component.html',
  styleUrl: './documentos.component.css'
})
export class DocumentosComponent {
  idSolicitud: number | null = null;
  archivoSeleccionado: File | null = null;
  documentos: Documento[] = [];

  cargando = false;
  mensajeError = '';
  mensajeExito = '';

  // Debe coincidir con las reglas de negocio de US-008 (también validadas en el backend)
  private extensionesPermitidas = ['pdf', 'jpg', 'jpeg', 'png'];
  private tamanoMaximoBytes = 5 * 1024 * 1024;

  constructor(private documentoService: DocumentoService) {}

  onArchivoSeleccionado(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    this.archivoSeleccionado = input.files?.[0] ?? null;
  }

  cargarDocumento(): void {
    this.limpiarMensajes();

    if (!this.idSolicitud) {
      this.mensajeError = 'Indica el ID de la solicitud a la que pertenece el documento.';
      return;
    }
    if (!this.archivoSeleccionado) {
      this.mensajeError = 'Selecciona un archivo antes de subirlo.';
      return;
    }

    // Validación en el cliente (UX rápida); el backend vuelve a validar por seguridad
    const extension = this.archivoSeleccionado.name.split('.').pop()?.toLowerCase() || '';
    if (!this.extensionesPermitidas.includes(extension)) {
      this.mensajeError = 'Formato no permitido. Solo se aceptan PDF, JPG y PNG.';
      return;
    }
    if (this.archivoSeleccionado.size > this.tamanoMaximoBytes) {
      this.mensajeError = 'El archivo supera el tamaño máximo permitido (5 MB).';
      return;
    }

    this.cargando = true;
    this.documentoService.cargar(this.idSolicitud, this.archivoSeleccionado).subscribe({
      next: () => {
        this.mensajeExito = 'Documento cargado correctamente.';
        this.archivoSeleccionado = null;
        this.cargando = false;
        this.consultarDocumentos();
      },
      error: (err) => {
        this.mensajeError = err?.error?.mensaje || 'No se pudo cargar el documento.';
        this.cargando = false;
      }
    });
  }

  consultarDocumentos(): void {
    this.limpiarMensajes();
    if (!this.idSolicitud) {
      this.mensajeError = 'Indica el ID de la solicitud para consultar sus documentos.';
      return;
    }

    this.cargando = true;
    this.documentoService.obtenerPorSolicitud(this.idSolicitud).subscribe({
      next: (data) => {
        this.documentos = data;
        this.cargando = false;
      },
      error: () => {
        this.mensajeError = 'No se pudieron cargar los documentos.';
        this.cargando = false;
      }
    });
  }

  private limpiarMensajes(): void {
    this.mensajeError = '';
    this.mensajeExito = '';
  }
}

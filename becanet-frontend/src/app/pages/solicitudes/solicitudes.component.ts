import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HeroBannerComponent } from '../../components/hero-banner/hero-banner.component';

interface SolicitudForm {
  idEstudiante: number | null;
  idConvocatoria: number | null;
  observaciones: string;
}

@Component({
  selector: 'app-solicitudes',
  standalone: true,
  imports: [CommonModule, FormsModule, HeroBannerComponent],
  templateUrl: './solicitudes.component.html',
  styleUrls: ['./solicitudes.component.scss']
})
export class SolicitudesComponent implements OnInit {
  private readonly DRAFT_KEY = 'becanet_solicitud_draft';

  formData: SolicitudForm = {
    idEstudiante: null,
    idConvocatoria: null,
    observaciones: ''
  };

  idEstudianteConsulta: number | null = null;
  
  // Estados de interfaz y feedback
  isSavingDraft = false;
  isSubmitting = false;
  isConsulting = false;
  feedbackMessage: { type: 'success' | 'danger' | 'info'; text: string } | null = null;
  hasDraft = false;

  solicitudesList: any[] = [];

  ngOnInit(): void {
    this.cargarBorrador();
  }

  // Guardado automático en localStorage
  onFormChange(): void {
    this.isSavingDraft = true;
    localStorage.setItem(this.DRAFT_KEY, JSON.stringify(this.formData));
    this.hasDraft = true;
    
    setTimeout(() => {
      this.isSavingDraft = false;
    }, 400);
  }

  cargarBorrador(): void {
    const saved = localStorage.getItem(this.DRAFT_KEY);
    if (saved) {
      try {
        this.formData = JSON.parse(saved);
        this.hasDraft = true;
      } catch {
        this.limpiarBorrador();
      }
    }
  }

  limpiarBorrador(): void {
    localStorage.removeItem(this.DRAFT_KEY);
    this.formData = { idEstudiante: null, idConvocatoria: null, observaciones: '' };
    this.hasDraft = false;
    this.mostrarFeedback('info', 'Se ha limpiado el borrador.');
  }

  crearSolicitud(): void {
    if (!this.formData.idEstudiante || !this.formData.idConvocatoria) {
      this.mostrarFeedback('danger', 'Por favor complete todos los campos obligatorios.');
      return;
    }

    this.isSubmitting = true;
    this.feedbackMessage = null;

    // Simulación de envío al Backend
    setTimeout(() => {
      this.isSubmitting = false;
      this.mostrarFeedback('success', 'Solicitud creada con éxito.');
      this.limpiarBorrador();
    }, 1200);
  }

  consultarSolicitudes(): void {
    if (!this.idEstudianteConsulta) {
      this.mostrarFeedback('danger', 'Ingrese un ID de estudiante válido.');
      return;
    }

    this.isConsulting = true;
    setTimeout(() => {
      this.isConsulting = false;
      this.solicitudesList = []; // Carga las solicitudes de la API
    }, 800);
  }

  private mostrarFeedback(type: 'success' | 'danger' | 'info', text: string): void {
    this.feedbackMessage = { type, text };
    setTimeout(() => {
      if (this.feedbackMessage?.text === text) {
        this.feedbackMessage = null;
      }
    }, 4000);
  }
}
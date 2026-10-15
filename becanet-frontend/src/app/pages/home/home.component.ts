import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SesionService } from '../../services/sesion.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  promedio: number | null = null;
  resultadoSimulador: string = '';

  constructor(
    private sesionService: SesionService,
    private router: Router
  ) {}

  postular(): void {
    if (!this.sesionService.estaAutenticado()) {
      this.router.navigate(['/login']);
    } else if (this.sesionService.obtenerRol() === 'ESTUDIANTE') {
      this.router.navigate(['/mi-solicitud']);
    }
  }

  esEstudianteOInvitado(): boolean {
    const rol = this.sesionService.obtenerRol();
    return !rol || rol === 'ESTUDIANTE';
  }

  simularElegibilidad(): void {
    if (this.promedio === null || this.promedio < 0 || this.promedio > 100) {
      this.resultadoSimulador = 'Por favor, ingresa un promedio válido entre 0 y 100.';
      return;
    }
    if (this.promedio >= 85) {
      this.resultadoSimulador = '¡Excelente! Calificas para Becas de Excelencia Académica y Apoyo Socioeconómico.';
    } else if (this.promedio >= 70) {
      this.resultadoSimulador = 'Calificas para Becas de Apoyo Socioeconómico y Becas Regionales.';
    } else {
      this.resultadoSimulador = 'Tu promedio actual no alcanza el mínimo sugerido (70 puntos), pero puedes consultar convocatorias especiales.';
    }
  }
}
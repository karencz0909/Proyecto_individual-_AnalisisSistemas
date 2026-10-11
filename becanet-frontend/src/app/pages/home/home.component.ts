import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

interface Beca {
  id: number;
  titulo: string;
  categoria: string;
  cobertura: string;
  requisitos: string;
  icono: string;
}

interface FAQ {
  pregunta: string;
  respuesta: string;
  abierto: boolean;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  // Filtro activo
  categoriaActiva: string = 'Todas';
  categorias: string[] = ['Todas', 'Académica', 'Socioeconómica', 'Posgrado', 'Especial'];

  // Listado de becas
  becas: Beca[] = [
    {
      id: 1,
      titulo: 'Beca Excelencia Académica',
      categoria: 'Académica',
      cobertura: '100% Colegiatura + Estipendio mensual',
      requisitos: 'Promedio mínimo de 85 puntos',
      icono: ''
    },
    {
      id: 2,
      titulo: 'Beca Apoyo Socioeconómico',
      categoria: 'Socioeconómica',
      cobertura: 'Hasta 80% de descuento en matrícula y apoyo de transporte',
      requisitos: 'Estudio socioeconómico acreditado',
      icono: ''
    },
    {
      id: 3,
      titulo: 'Beca Posgrados e Investigación',
      categoria: 'Posgrado',
      cobertura: 'Financiamiento total para maestrías y tesis',
      requisitos: 'Título universitario y propuesta de investigación',
      icono: ''
    },
    {
      id: 4,
      titulo: 'Beca Talento Deportivo y Artístico',
      categoria: 'Especial',
      cobertura: '50% a 100% de beca según rendimiento',
      requisitos: 'Acreditación de logros destacados',
      icono: ''
    }
  ];

  // Interacción: Simulador de elegibilidad
  promedioInput: number | null = null;
  resultadoSimulacion: string | null = null;

  // Interacción: Preguntas Frecuentes (Acordeón)
  faqs: FAQ[] = [
    {
      pregunta: '¿Cuáles son los requisitos generales para postular a una beca?',
      respuesta: 'Ser estudiante activo, contar con papelería digitalizada actualizada (DPI, expediente académico) y no poseer otra beca gubernamental activa.',
      abierto: false
    },
    {
      pregunta: '¿Cómo le doy seguimiento al estado de mi solicitud?',
      respuesta: 'Una vez que inicies sesión en la plataforma BecaNet, tendrás acceso a tu panel "Mi Solicitud" para ver el progreso en tiempo real (En Evaluación, Aprobado o Rechazado).',
      abierto: false
    },
    {
      pregunta: '¿Tiene algún costo realizar el proceso de postulación?',
      respuesta: 'No. Todos los trámites de solicitud y asignación de becas dentro de BecaNet son 100% gratuitos y transparentes.',
      abierto: false
    }
  ];

  get becasFiltradas(): Beca[] {
    if (this.categoriaActiva === 'Todas') {
      return this.becas;
    }
    return this.becas.filter(b => b.categoria === this.categoriaActiva);
  }

  seleccionarCategoria(categoria: string): void {
    this.categoriaActiva = categoria;
  }

  toggleFAQ(index: number): void {
    this.faqs[index].abierto = !this.faqs[index].abierto;
  }

  evaluarPromedio(): void {
    if (!this.promedioInput || this.promedioInput < 0 || this.promedioInput > 100) {
      this.resultadoSimulacion = 'Ingresa un promedio válido entre 0 y 100.';
      return;
    }

    if (this.promedioInput >= 85) {
      this.resultadoSimulacion = '¡Excelente! Calificas para la Beca de Excelencia Académica y Apoyo Socioeconómico.';
    } else if (this.promedioInput >= 70) {
      this.resultadoSimulacion = 'Calificas para la Beca Socioeconómica y Programas de Apoyo Especial.';
    } else {
      this.resultadoSimulacion = 'Te recomendamos revisar los programas de tutorías y nivelación académica.';
    }
  }
}

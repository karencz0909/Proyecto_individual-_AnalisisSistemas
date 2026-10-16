import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './registro.component.html',
  styleUrl: '../login/login.component.css'
})
export class RegistroComponent {
  datos = {
    nombre: '',
    correo: '',
    contrasena: '',
    telefono: '',
    nivelAcademico: 'Universitario',
    institucionProcedencia: ''
  };
  mensajeError = '';
  cargando = false;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    this.mensajeError = '';

    if (!this.datos.nombre.trim() || !this.datos.correo.trim()) {
      this.mensajeError = 'Completa tu nombre y tu correo.';
      return;
    }
    if (this.datos.contrasena.length < 8) {
      this.mensajeError = 'La contraseña debe tener al menos 8 caracteres.';
      return;
    }

    this.cargando = true;
    this.authService.registrarEstudiante(this.datos).subscribe({
      next: () => {
        this.router.navigate(['/login'], { queryParams: { registrado: 'true' } });
      },
      error: (err) => {
        this.mensajeError = err.error?.mensaje || 'No se pudo crear la cuenta.';
        this.cargando = false;
      }
    });
  }
}
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './registro.component.html'
})
export class RegistroComponent {
  datos = {
    nombre: '',
    correo: '',
    contrasena: '',
    telefono: '',
    nivelAcademico: 'Grado',
    institucionProcedencia: ''
  };
  mensajeError = '';
  cargando = false;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.cargando = true;
    this.mensajeError = '';
    this.authService.registrarEstudiante(this.datos).subscribe({
      next: () => {
        this.router.navigate(['/login'], { queryParams: { registrado: 'true' } });
      },
      error: (err) => {
        this.mensajeError = err.error?.mensaje || 'Error al registrar la cuenta.';
        this.cargando = false;
      }
    });
  }
}
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { SesionService } from '../../services/sesion.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  credenciales = { correo: '', contrasena: '' };
  mensajeError = '';
  cargando = false;

  constructor(
    private authService: AuthService,
    private sesionService: SesionService,
    private router: Router
  ) {}

  // Método invocado por (ngSubmit)="iniciarSesion()" en la plantilla HTML
  iniciarSesion() {
    this.cargando = true;
    this.mensajeError = '';

    this.authService.login(this.credenciales).subscribe({
      next: (res) => {
        this.sesionService.iniciarSesion(res);
        this.redirigirSegunRol(res.role);
      },
      error: (err) => {
        this.mensajeError = err.error?.mensaje || 'Credenciales inválidas';
        this.cargando = false;
      }
    });
  }

  // Alias por si el HTML utiliza onSubmit()
  onSubmit() {
    this.iniciarSesion();
  }

  private redirigirSegunRol(rol: string) {
    switch (rol) {
      case 'ESTUDIANTE':
        this.router.navigate(['/mi-solicitud']);
        break;
      case 'COORDINADOR':
        this.router.navigate(['/solicitudes-admin']);
        break;
      case 'EVALUADOR':
        this.router.navigate(['/evaluaciones']);
        break;
      default:
        this.router.navigate(['/']);
        break;
    }
  }
}
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { SesionService } from '../../services/sesion.service';
import { LoginRequest, AuthRespuesta } from '../../models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  credenciales: LoginRequest = { correo: '', contrasena: '' };
  cargando = false;
  mensajeError = '';

  constructor(
    private authService: AuthService,
    private sesionService: SesionService,
    private router: Router
  ) {}

  iniciarSesion(): void {
    this.mensajeError = '';

    if (!this.credenciales.correo.trim() || !this.credenciales.contrasena.trim()) {
      this.mensajeError = 'Completa tu correo y tu contraseña para continuar.';
      return;
    }

    this.cargando = true;
    this.authService.login(this.credenciales).subscribe({
      next: (respuesta: AuthRespuesta) => {
        this.sesionService.iniciarSesion(respuesta);
        this.cargando = false;
        this.redirigirSegunRol(respuesta.tipoUsuario);
      },
      error: (err: any) => {
        this.mensajeError = err?.error?.mensaje || 'Correo o contraseña incorrectos.';
        this.cargando = false;
      }
    });
  }

  private redirigirSegunRol(tipoUsuario: string): void {
    switch (tipoUsuario) {
      case 'ADMIN':
        this.router.navigate(['/solicitudes-admin']);
        break;
      case 'ESTUDIANTE':
        this.router.navigate(['/mi-solicitud']);
        break;
      case 'COORDINADOR':
        this.router.navigate(['/comites']);
        break;
      case 'EVALUADOR':
        this.router.navigate(['/evaluaciones']);
        break;
      default:
        this.router.navigate(['/login']);
    }
  }
}
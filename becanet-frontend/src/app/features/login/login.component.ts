import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { SesionService } from '../../services/sesion.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  credenciales = { correo: '', contrasena: '' };
  mensajeError = '';
  mensajeInfo = '';
  cargando = false;

  constructor(
    private authService: AuthService,
    private sesionService: SesionService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    if (params.get('registrado') === 'true') {
      this.mensajeInfo = 'Tu cuenta fue creada. Inicia sesión para continuar.';
    } else if (params.get('sesionExpirada') === 'true') {
      this.mensajeInfo = 'Tu sesión expiró. Vuelve a iniciar sesión; tus borradores se conservaron.';
    }
  }

  iniciarSesion(): void {
    this.mensajeError = '';
    this.mensajeInfo = '';

    if (!this.credenciales.correo.trim() || !this.credenciales.contrasena.trim()) {
      this.mensajeError = 'Completa tu correo y tu contraseña para continuar.';
      return;
    }

    this.cargando = true;
    this.authService.login(this.credenciales).subscribe({
      next: (res) => {
        // El backend responde con "tipoUsuario"; SesionService lo guarda como "role".
        this.sesionService.iniciarSesion({
          token: res.token,
          role: res.tipoUsuario,
          nombre: res.nombre,
          correo: res.correo,
          id: String(res.id)
        });
        this.cargando = false;
        this.redirigirSegunRol(res.tipoUsuario);
      },
            error: (err) => {
        if (err.status === 0) {
          this.mensajeError = 'No se pudo conectar con el servidor. Verifica que el backend esté corriendo.';
        } else if (err.status === 401) {
          this.mensajeError = err.error?.mensaje || 'Correo o contraseña incorrectos.';
        } else {
          this.mensajeError = 'Ocurrió un error en el servidor. Intenta de nuevo en un momento.';
        }
        this.cargando = false;
      }
    });
  }

  private redirigirSegunRol(rol: string): void {
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
    }
  }
}
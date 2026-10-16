import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SesionService } from '../../services/sesion.service';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {

  constructor(
    private sesionService: SesionService,
    private router: Router,
    public themeService: ThemeService
  ) {}

  get estaAutenticado(): boolean {
    return this.sesionService.estaAutenticado();
  }

  get rol(): string | null {
    return this.sesionService.obtenerRol();
  }

  get nombreUsuario(): string {
    return this.sesionService.obtenerNombre();
  }

  get iniciales(): string {
    const nombre = this.nombreUsuario.trim();
    if (!nombre || nombre === 'Usuario') return 'US';
    const partes = nombre.split(' ');
    if (partes.length >= 2) {
      return (partes[0][0] + partes[1][0]).toUpperCase();
    }
    return nombre.substring(0, 2).toUpperCase();
  }

  esPaginaAuth(): boolean {
    const url = this.router.url.split('?')[0];
    return url === '/login' || url === '/registro';
  }

  esPaginaPrincipal(): boolean {
    const urlLimpia = this.router.url.split('#')[0].split('?')[0];
    return urlLimpia === '/' || urlLimpia === '';
  }

  cerrarSesion(): void {
    this.sesionService.cerrarSesion();
    this.router.navigate(['/']);
  }
}
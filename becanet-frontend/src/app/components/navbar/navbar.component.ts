import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  constructor(
    public themeService: ThemeService,
    private router: Router
  ) {}

  // Retorna true únicamente cuando el usuario está en la página principal ('/')
  esPaginaPrincipal(): boolean {
    return this.router.url === '/' || this.router.url === '/home';
  }

  // Verifica si hay una sesión activa
  estaAutenticado(): boolean {
    return !!localStorage.getItem('token');
  }
}
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SesionService {
  private readonly TOKEN_KEY = 'becanet_token';
  private readonly ROLE_KEY = 'becanet_role';
  private readonly NOMBRE_KEY = 'becanet_nombre';
  private readonly CORREO_KEY = 'becanet_correo';
  private readonly ID_KEY = 'becanet_id';

  iniciarSesion(usuario: { token: string; role: string; nombre?: string; correo?: string; id?: string }) {
    localStorage.setItem(this.TOKEN_KEY, usuario.token);
    localStorage.setItem(this.ROLE_KEY, usuario.role);
    if (usuario.nombre) localStorage.setItem(this.NOMBRE_KEY, usuario.nombre);
    if (usuario.correo) localStorage.setItem(this.CORREO_KEY, usuario.correo);
    if (usuario.id) localStorage.setItem(this.ID_KEY, usuario.id);
  }

  obtenerToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  obtenerRol(): string | null {
    return localStorage.getItem(this.ROLE_KEY);
  }

  obtenerNombre(): string {
    return localStorage.getItem(this.NOMBRE_KEY) || 'Usuario';
  }

  obtenerId(): string | null {
    return localStorage.getItem(this.ID_KEY);
  }

  estaAutenticado(): boolean {
    return !!this.obtenerToken();
  }

  cerrarSesion() {
    // Solamente se borran las claves de autenticación, preservando becanet_theme y borradores
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.ROLE_KEY);
    localStorage.removeItem(this.NOMBRE_KEY);
    localStorage.removeItem(this.CORREO_KEY);
    localStorage.removeItem(this.ID_KEY);
  }
}

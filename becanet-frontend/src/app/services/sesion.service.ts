import { Injectable } from '@angular/core';
import { AuthRespuesta, TipoUsuario } from '../models/auth.model'; // <-- Usar ../ en lugar de ../../

@Injectable({ providedIn: 'root' })
export class SesionService {
  iniciarSesion(respuesta: AuthRespuesta): void {
    localStorage.setItem('token', respuesta.token);
    localStorage.setItem('role', respuesta.tipoUsuario);
  }

  obtenerTipoUsuario(): string | null {
    return localStorage.getItem('role');
  }

  cerrarSesion(): void {
    localStorage.clear();
  }
}
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CancelarSolicitud,
  CrearMiSolicitud,
  CrearSolicitud,
  ResolverSolicitud,
  Solicitud
} from '../models/solicitud.model';

@Injectable({ providedIn: 'root' })
export class SolicitudService {
  private baseUrl = `${environment.apiUrl}/solicitudes`;

  constructor(private http: HttpClient) {}

  // --- Estudiante autenticado (el id sale del token, no se escribe) ---
  crearMia(dto: CrearMiSolicitud): Observable<Solicitud> {
    return this.http.post<Solicitud>(`${this.baseUrl}/mia`, dto);
  }

  obtenerMias(): Observable<Solicitud[]> {
    return this.http.get<Solicitud[]>(`${this.baseUrl}/mias`);
  }

  cancelar(id: number, dto: CancelarSolicitud): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/cancelar`, dto);
  }

  // --- Coordinador ---
  resolver(id: number, dto: ResolverSolicitud): Observable<Solicitud> {
    return this.http.put<Solicitud>(`${this.baseUrl}/${id}/resolver`, dto);
  }

  // --- Versiones con id explícito (se conservan por compatibilidad) ---
  crear(dto: CrearSolicitud): Observable<Solicitud> {
    return this.http.post<Solicitud>(this.baseUrl, dto);
  }

  obtenerPorId(id: number): Observable<Solicitud> {
    return this.http.get<Solicitud>(`${this.baseUrl}/${id}`);
  }

  obtenerPorEstudiante(idEstudiante: number): Observable<Solicitud[]> {
    return this.http.get<Solicitud[]>(`${this.baseUrl}/estudiante/${idEstudiante}`);
  }
}

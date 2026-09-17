import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CancelarSolicitud, CrearSolicitud, Solicitud } from '../models/solicitud.model';

@Injectable({ providedIn: 'root' })
export class SolicitudService {
  private baseUrl = `${environment.apiUrl}/solicitudes`;

  constructor(private http: HttpClient) {}

  // US-007: crear una solicitud de beca
  crear(dto: CrearSolicitud): Observable<Solicitud> {
    return this.http.post<Solicitud>(this.baseUrl, dto);
  }

  obtenerPorId(id: number): Observable<Solicitud> {
    return this.http.get<Solicitud>(`${this.baseUrl}/${id}`);
  }

  obtenerPorEstudiante(idEstudiante: number): Observable<Solicitud[]> {
    return this.http.get<Solicitud[]>(`${this.baseUrl}/estudiante/${idEstudiante}`);
  }

  // US-009: cancelar una solicitud
  cancelar(id: number, dto: CancelarSolicitud): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/cancelar`, dto);
  }
}

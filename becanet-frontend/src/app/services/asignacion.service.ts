import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AsignacionEvaluador,
  EvaluadorOpcion,
  ResultadoDistribucion,
  RevisarSolicitud,
  SolicitudCoordinador
} from '../models/asignacion.model';

@Injectable({ providedIn: 'root' })
export class AsignacionService {
  private baseUrl = `${environment.apiUrl}/asignaciones`;

  constructor(private http: HttpClient) {}

  // --- Evaluador ---
  obtenerMias(): Observable<AsignacionEvaluador[]> {
    return this.http.get<AsignacionEvaluador[]>(`${this.baseUrl}/mias`);
  }

  revisar(idSolicitud: number, dto: RevisarSolicitud): Observable<AsignacionEvaluador> {
    return this.http.put<AsignacionEvaluador>(`${this.baseUrl}/${idSolicitud}/revisar`, dto);
  }

  reabrir(idSolicitud: number): Observable<AsignacionEvaluador> {
    return this.http.put<AsignacionEvaluador>(`${this.baseUrl}/${idSolicitud}/reabrir`, {});
  }

  // --- Coordinador ---
  obtenerSolicitudes(): Observable<SolicitudCoordinador[]> {
    return this.http.get<SolicitudCoordinador[]>(`${this.baseUrl}/solicitudes`);
  }

  obtenerEvaluadores(): Observable<EvaluadorOpcion[]> {
    return this.http.get<EvaluadorOpcion[]>(`${this.baseUrl}/evaluadores`);
  }

  distribuir(idComite: number | null): Observable<ResultadoDistribucion> {
    return this.http.post<ResultadoDistribucion>(`${this.baseUrl}/distribuir`, { idComite });
  }

  asignar(idSolicitud: number, idEvaluador: number): Observable<SolicitudCoordinador> {
    return this.http.put<SolicitudCoordinador>(`${this.baseUrl}/${idSolicitud}/asignar`, { idEvaluador });
  }
}

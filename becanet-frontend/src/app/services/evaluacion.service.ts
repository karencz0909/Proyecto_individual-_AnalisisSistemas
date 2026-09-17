import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Evaluacion, RegistrarEvaluacion } from '../models/evaluacion.model';

@Injectable({ providedIn: 'root' })
export class EvaluacionService {
  private baseUrl = `${environment.apiUrl}/evaluaciones`;

  constructor(private http: HttpClient) {}

  // US-012: registrar una evaluación
  registrar(dto: RegistrarEvaluacion): Observable<Evaluacion> {
    return this.http.post<Evaluacion>(this.baseUrl, dto);
  }

  obtenerPorSolicitud(idSolicitud: number): Observable<Evaluacion[]> {
    return this.http.get<Evaluacion[]>(`${this.baseUrl}/solicitud/${idSolicitud}`);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AsignarSolicitudes, Comite, CrearComite } from '../models/comite.model';

@Injectable({ providedIn: 'root' })
export class ComiteService {
  private baseUrl = `${environment.apiUrl}/comites`;

  constructor(private http: HttpClient) {}

  // US-010: crear comité con sus miembros
  crear(dto: CrearComite): Observable<Comite> {
    return this.http.post<Comite>(this.baseUrl, dto);
  }

  obtenerTodos(): Observable<Comite[]> {
    return this.http.get<Comite[]>(this.baseUrl);
  }

  obtenerPorId(id: number): Observable<Comite> {
    return this.http.get<Comite>(`${this.baseUrl}/${id}`);
  }

  // US-011: asignar solicitudes a un comité
  asignarSolicitudes(dto: AsignarSolicitudes): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/asignar-solicitudes`, dto);
  }
}

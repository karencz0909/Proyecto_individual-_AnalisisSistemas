import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Documento } from '../models/documento.model';

@Injectable({ providedIn: 'root' })
export class DocumentoService {
  private baseUrl = `${environment.apiUrl}/documentos`;

  constructor(private http: HttpClient) {}

  // US-008: cargar un documento (multipart/form-data)
  cargar(idSolicitud: number, archivo: File): Observable<Documento> {
    const formData = new FormData();
    formData.append('archivo', archivo, archivo.name);
    return this.http.post<Documento>(`${this.baseUrl}/solicitud/${idSolicitud}`, formData);
  }

  obtenerPorSolicitud(idSolicitud: number): Observable<Documento[]> {
    return this.http.get<Documento[]>(`${this.baseUrl}/solicitud/${idSolicitud}`);
  }
}

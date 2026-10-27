import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Documento } from '../models/documento.model';

@Injectable({ providedIn: 'root' })
export class DocumentoService {
  private baseUrl = `${environment.apiUrl}/documentos`;

  constructor(private http: HttpClient) {}

  /** tipoDocumento: CERTIFICACION_ESTUDIOS (obligatorio) u OTRO. */
  cargar(idSolicitud: number, archivo: File, tipoDocumento: string = 'OTRO'): Observable<Documento> {
    const formData = new FormData();
    formData.append('archivo', archivo, archivo.name);
    formData.append('tipoDocumento', tipoDocumento);
    return this.http.post<Documento>(`${this.baseUrl}/solicitud/${idSolicitud}`, formData);
  }

  obtenerPorSolicitud(idSolicitud: number): Observable<Documento[]> {
    return this.http.get<Documento[]>(`${this.baseUrl}/solicitud/${idSolicitud}`);
  }

  /** Los archivos viven en el backend (http://localhost:5000/uploads/...), no en Angular. */
  urlCompleta(urlRelativa: string): string {
    const origen = environment.apiUrl.replace(/\/api\/?$/, '');
    return `${origen}${urlRelativa}`;
  }
}

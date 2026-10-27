import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Convocatoria } from '../models/convocatoria.model';

@Injectable({ providedIn: 'root' })
export class ConvocatoriaService {
  private baseUrl = `${environment.apiUrl}/convocatorias`;

  constructor(private http: HttpClient) {}

  obtenerAbiertas(): Observable<Convocatoria[]> {
    return this.http.get<Convocatoria[]>(`${this.baseUrl}/abiertas`);
  }
}

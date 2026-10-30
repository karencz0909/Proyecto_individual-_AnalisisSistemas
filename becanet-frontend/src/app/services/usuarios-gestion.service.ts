import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DatosUsuarioGestion, TipoUsuarioGestion, UsuarioGestion } from '../models/usuario-gestion.model';

@Injectable({ providedIn: 'root' })
export class UsuariosGestionService {
  private readonly url = `${environment.apiUrl}/usuarios`;

  constructor(private http: HttpClient) {}

  listar(): Observable<UsuarioGestion[]> {
    return this.http.get<UsuarioGestion[]>(this.url);
  }

  crear(datos: DatosUsuarioGestion): Observable<UsuarioGestion> {
    return this.http.post<UsuarioGestion>(this.url, datos);
  }

  actualizar(id: number, tipo: TipoUsuarioGestion, datos: DatosUsuarioGestion): Observable<UsuarioGestion> {
    return this.http.put<UsuarioGestion>(`${this.url}/${id}/${tipo}`, datos);
  }
}

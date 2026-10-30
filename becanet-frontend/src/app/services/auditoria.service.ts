import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { RegistroAuditoria } from '../models/registro-auditoria.model';

@Injectable({ providedIn: 'root' })
export class AuditoriaService {
  private readonly url = `${environment.apiUrl}/auditoria`;
  constructor(private http: HttpClient) {}
  obtenerRecientes(): Observable<RegistroAuditoria[]> {
    return this.http.get<RegistroAuditoria[]>(this.url);
  }
}

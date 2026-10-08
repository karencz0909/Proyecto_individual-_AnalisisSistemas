import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequest, AuthRespuesta } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient) {}

  login(credenciales: LoginRequest): Observable<AuthRespuesta> {
    return this.http.post<AuthRespuesta>('/api/login', credenciales);
  }
}
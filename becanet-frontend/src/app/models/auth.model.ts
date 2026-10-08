export interface LoginRequest {
  correo: string;
  contrasena: string;
}

export interface RegistroEstudianteRequest {
  nombre: string;
  correo: string;
  contrasena: string;
  telefono?: string;
  nivelAcademico: string;
  institucionProcedencia?: string;
}

export type TipoUsuario = 'ESTUDIANTE' | 'COORDINADOR' | 'EVALUADOR';

export interface AuthRespuesta {
  token: string;
  id: number;
  nombre: string;
  correo: string;
  tipoUsuario: TipoUsuario;
}
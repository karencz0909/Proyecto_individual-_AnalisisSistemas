export type TipoUsuarioGestion = 'ESTUDIANTE' | 'COORDINADOR' | 'EVALUADOR';

export interface UsuarioGestion {
  id: number;
  nombre: string;
  correo: string;
  telefono?: string | null;
  tipoUsuario: TipoUsuarioGestion;
  fechaRegistro: string;
  nivelAcademico?: string | null;
  institucionProcedencia?: string | null;
  cargo?: string | null;
  especialidad?: string | null;
}

export interface DatosUsuarioGestion {
  nombre: string;
  correo: string;
  contrasena: string;
  tipoUsuario: TipoUsuarioGestion;
  telefono: string;
  nivelAcademico: string;
  institucionProcedencia: string;
  cargo: string;
  especialidad: string;
}

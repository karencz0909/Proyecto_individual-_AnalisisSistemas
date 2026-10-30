export interface RegistroAuditoria {
  id: number;
  idSolicitud: number | null;
  entidad: string;
  idEntidad: string | null;
  idUsuario: number;
  rolUsuario: string;
  nombreUsuario: string;
  correoUsuario: string;
  operacion: string;
  detalle: string | null;
  fechaHoraUtc: string;
}

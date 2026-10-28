export interface Solicitud {
  id: number;
  fechaCreacion: string;
  estado: string;
  estadoRevision?: string;
  observaciones?: string;
  motivo?: string;
  montoSolicitado: number;
  motivoResolucion?: string;
  fechaResolucion?: string;
  idEstudiante: number;
  nombreEstudiante?: string;
  idConvocatoria: number;
  tituloConvocatoria?: string;
  idComite?: number;
  cantidadDocumentos: number;
  tieneCertificacionEstudios?: boolean;
  aprobaciones?: AprobacionSolicitud[];
}

export interface AprobacionSolicitud {
  tipo: string;
  idUsuario: number;
  rolUsuario: string;
  aprobada: boolean;
  motivo?: string;
  fechaHoraUtc: string;
}

export interface NotificacionSolicitud {
  id: number;
  idSolicitud: number;
  estadoNuevo: string;
  mensaje: string;
  fechaHoraUtc: string;
}

export interface CrearSolicitud {
  idEstudiante: number;
  idConvocatoria: number;
  montoSolicitado: number;
  observaciones?: string;
}

/** Lo que envía el estudiante autenticado: su id sale del token. */
export interface CrearMiSolicitud {
  idConvocatoria: number;
  motivo: string;
  montoSolicitado: number;
}

export interface CancelarSolicitud {
  motivo?: string;
}

export interface ResolverSolicitud {
  aprobar: boolean;
  motivo?: string;
}

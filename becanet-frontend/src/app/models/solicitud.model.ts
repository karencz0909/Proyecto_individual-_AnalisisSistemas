export interface Solicitud {
  id: number;
  fechaCreacion: string;
  estado: string;
  estadoRevision?: string;
  observaciones?: string;
  motivo?: string;
  motivoResolucion?: string;
  fechaResolucion?: string;
  idEstudiante: number;
  nombreEstudiante?: string;
  idConvocatoria: number;
  tituloConvocatoria?: string;
  idComite?: number;
  cantidadDocumentos: number;
  tieneCertificacionEstudios?: boolean;
}

export interface CrearSolicitud {
  idEstudiante: number;
  idConvocatoria: number;
  observaciones?: string;
}

/** Lo que envía el estudiante autenticado: su id sale del token. */
export interface CrearMiSolicitud {
  idConvocatoria: number;
  motivo: string;
}

export interface CancelarSolicitud {
  motivo?: string;
}

export interface ResolverSolicitud {
  aprobar: boolean;
  motivo?: string;
}

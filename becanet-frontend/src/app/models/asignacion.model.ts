export interface DocumentoResumen {
  id: number;
  nombreArchivo: string;
  tipoDocumento: string;
  urlArchivo: string;
}

/** Solicitud tal como la ve el evaluador al que se le asignó. */
export interface AsignacionEvaluador {
  idSolicitud: number;
  nombreEstudiante: string;
  tituloConvocatoria: string;
  motivo?: string;
  montoSolicitado: number;
  decisionEvaluadorRegistrada: boolean;
  aprobadaPorEvaluador: boolean;
  estadoSolicitud: string;
  estadoRevision: string; // EN_REVISION | REVISADO
  fechaAsignacion?: string;
  puntaje?: number | null;
  observacionesEvaluacion?: string | null;
  documentos: DocumentoResumen[];
}

export interface RevisarSolicitud {
  puntaje: number;
  observaciones: string;
}

/** Solicitud tal como la ve el coordinador. */
export interface SolicitudCoordinador {
  idSolicitud: number;
  fechaCreacion: string;
  nombreEstudiante: string;
  correoEstudiante: string;
  tituloConvocatoria: string;
  motivo?: string;
  montoSolicitado: number;
  aprobadaPorEvaluador: boolean;
  aprobadaPorCoordinador: boolean;
  estado: string;
  estadoRevision: string; // SIN_ASIGNAR | EN_REVISION | REVISADO
  idEvaluadorAsignado?: number | null;
  nombreEvaluador?: string | null;
  correoEvaluador?: string | null;
  puntaje?: number | null;
  observacionesEvaluacion?: string | null;
  motivoResolucion?: string | null;
  tieneCertificacionEstudios: boolean;
  documentos: DocumentoResumen[];
}

export interface EvaluadorOpcion {
  id: number;
  nombre: string;
  correo: string;
  asignadas: number;
}

export interface ResultadoDistribucion {
  totalAsignadas: number;
  omitidasSinCertificacion: number;
  porEvaluador: EvaluadorOpcion[];
}

export interface Solicitud {
  id: number;
  fechaCreacion: string;
  estado: string;
  observaciones?: string;
  idEstudiante: number;
  nombreEstudiante?: string;
  idConvocatoria: number;
  tituloConvocatoria?: string;
  idComite?: number;
  cantidadDocumentos: number;
}

export interface CrearSolicitud {
  idEstudiante: number;
  idConvocatoria: number;
  observaciones?: string;
}

export interface CancelarSolicitud {
  motivo?: string;
}

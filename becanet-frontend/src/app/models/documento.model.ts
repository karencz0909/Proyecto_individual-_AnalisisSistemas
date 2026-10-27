export type TipoDocumento = 'CERTIFICACION_ESTUDIOS' | 'OTRO';

export interface Documento {
  id: number;
  nombreArchivo: string;
  tipoArchivo: string;
  tipoDocumento?: TipoDocumento | string;
  urlArchivo: string;
  fechaCarga: string;
  idSolicitud: number;
}

export interface Convocatoria {
  id: number;
  titulo: string;
  descripcion?: string;
  requisitos?: string;
  fechaApertura: string;
  fechaCierre: string;
  estado: string;
  cantidadSolicitudes: number;
}

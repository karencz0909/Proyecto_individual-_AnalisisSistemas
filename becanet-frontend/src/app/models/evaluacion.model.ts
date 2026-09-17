export interface Evaluacion {
  id: number;
  puntaje: number;
  observaciones?: string;
  fechaEvaluacion: string;
  idSolicitud: number;
  idEvaluador: number;
  nombreEvaluador?: string;
}

export interface RegistrarEvaluacion {
  idSolicitud: number;
  idEvaluador: number;
  puntaje: number;
  observaciones: string;
}

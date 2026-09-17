export interface Comite {
  id: number;
  nombre: string;
  fechaCreacion: string;
  miembros: string[];
  solicitudesAsignadas: number;
}

export interface CrearComite {
  nombre: string;
  idsEvaluadores: number[];
}

export interface AsignarSolicitudes {
  idComite: number;
  idsSolicitudes: number[];
}

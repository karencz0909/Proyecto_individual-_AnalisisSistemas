import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { DatosUsuarioGestion, TipoUsuarioGestion, UsuarioGestion } from '../../models/usuario-gestion.model';
import { UsuariosGestionService } from '../../services/usuarios-gestion.service';

type TipoAviso = 'success' | 'error';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './usuarios.component.html',
  styleUrls: ['../../shared/estilos-paginas.css', './usuarios.component.css']
})
export class UsuariosComponent implements OnInit {
  usuarios: UsuarioGestion[] = [];
  formulario = this.formularioVacio();
  editandoId: number | null = null;
  busqueda = '';
  cargando = true;
  guardando = false;
  aviso: { tipo: TipoAviso; mensaje: string } | null = null;

  readonly roles: { valor: TipoUsuarioGestion; etiqueta: string }[] = [
    { valor: 'ESTUDIANTE', etiqueta: 'Estudiante' },
    { valor: 'COORDINADOR', etiqueta: 'Coordinador' },
    { valor: 'EVALUADOR', etiqueta: 'Evaluador' }
  ];

  constructor(private usuariosService: UsuariosGestionService) {}

  ngOnInit(): void { this.cargar(); }

  get usuariosFiltrados(): UsuarioGestion[] {
    const q = this.busqueda.trim().toLocaleLowerCase();
    if (!q) return this.usuarios;
    return this.usuarios.filter(u =>
      [u.nombre, u.correo, this.etiquetaRol(u.tipoUsuario)].some(v => v.toLocaleLowerCase().includes(q))
    );
  }

  etiquetaRol(rol: TipoUsuarioGestion): string {
    return this.roles.find(r => r.valor === rol)?.etiqueta ?? rol;
  }

  cargar(): void {
    this.cargando = true;
    this.usuariosService.listar().subscribe({
      next: usuarios => { this.usuarios = usuarios; this.cargando = false; },
      error: err => { this.aviso = { tipo: 'error', mensaje: this.errorDe(err) }; this.cargando = false; }
    });
  }

  cambiarRol(): void {
    this.formulario.nivelAcademico = this.formulario.tipoUsuario === 'ESTUDIANTE' ? 'Grado' : '';
    this.formulario.institucionProcedencia = '';
    this.formulario.cargo = '';
    this.formulario.especialidad = '';
  }

  editar(usuario: UsuarioGestion): void {
    this.editandoId = usuario.id;
    this.formulario = {
      nombre: usuario.nombre,
      correo: usuario.correo,
      contrasena: '',
      tipoUsuario: usuario.tipoUsuario,
      telefono: usuario.telefono ?? '',
      nivelAcademico: usuario.nivelAcademico ?? 'Grado',
      institucionProcedencia: usuario.institucionProcedencia ?? '',
      cargo: usuario.cargo ?? '',
      especialidad: usuario.especialidad ?? ''
    };
    this.aviso = null;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelarEdicion(form: NgForm): void {
    this.editandoId = null;
    this.formulario = this.formularioVacio();
    form.resetForm(this.formulario);
    this.aviso = null;
  }

  guardar(form: NgForm): void {
    if (form.invalid || this.guardando) return;
    this.aviso = null;
    this.guardando = true;
    const id = this.editandoId;
    const solicitud = id === null
      ? this.usuariosService.crear(this.formulario)
      : this.usuariosService.actualizar(id, this.formulario.tipoUsuario, this.formulario);

    solicitud.subscribe({
      next: usuario => {
        if (id === null) {
          this.usuarios = [...this.usuarios, usuario].sort((a, b) => a.nombre.localeCompare(b.nombre));
          this.aviso = { tipo: 'success', mensaje: `Se creó el perfil de ${usuario.nombre}.` };
        } else {
          this.usuarios = this.usuarios.map(u => u.id === id ? usuario : u);
          this.aviso = { tipo: 'success', mensaje: `Se actualizaron los datos de ${usuario.nombre}.` };
        }
        this.guardando = false;
        this.editandoId = null;
        this.formulario = this.formularioVacio();
        form.resetForm(this.formulario);
      },
      error: err => {
        this.guardando = false;
        this.aviso = { tipo: 'error', mensaje: this.errorDe(err) };
      }
    });
  }

  private formularioVacio(): DatosUsuarioGestion {
    return {
      nombre: '', correo: '', contrasena: '', tipoUsuario: 'ESTUDIANTE', telefono: '',
      nivelAcademico: 'Grado', institucionProcedencia: '', cargo: '', especialidad: ''
    };
  }

  private errorDe(err: any): string {
    const detalle = err?.error?.mensaje || err?.error?.detail || err?.error?.title;
    if (detalle) return detalle;
    if (err?.status === 0) return 'No se pudo conectar con el servidor. Verifica que el backend esté corriendo.';
    if (err?.status === 401) return 'Tu sesión expiró. Inicia sesión de nuevo como coordinador.';
    if (err?.status === 403) return 'Tu cuenta no tiene permiso para gestionar perfiles.';
    if (err?.status === 404) return 'El backend aún no reconoce la gestión de usuarios. Detén y vuelve a iniciar la API con dotnet run.';
    if (err?.status >= 500) return `El backend respondió con error ${err.status}. Revisa el mensaje rojo del terminal de la API; podría ser un problema de base de datos.`;
    return err?.status ? `La solicitud falló (HTTP ${err.status}). Revisa los datos e intenta de nuevo.` : 'No se pudo guardar la información. Intenta de nuevo.';
  }
}

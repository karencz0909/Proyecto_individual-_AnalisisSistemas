import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { AuditoriaService } from '../../services/auditoria.service';
import { RegistroAuditoria } from '../../models/registro-auditoria.model';

@Component({
  selector: 'app-auditoria',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './auditoria.component.html',
  styleUrls: ['../../shared/estilos-paginas.css', './auditoria.component.css']
})
export class AuditoriaComponent implements OnInit {
  registros: RegistroAuditoria[] = [];
  cargando = true;
  error = '';

  constructor(private auditoria: AuditoriaService) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando = true;
    this.error = '';
    this.auditoria.obtenerRecientes().subscribe({
      next: rows => { this.registros = rows; this.cargando = false; },
      error: err => {
        this.cargando = false;
        this.error = err?.error?.mensaje ?? 'No se pudieron cargar los registros de auditoría.';
      }
    });
  }
}

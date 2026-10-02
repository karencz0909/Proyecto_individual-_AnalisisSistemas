import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-comites',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comites.component.html',
  styleUrl: './comites.component.css'
})
export class ComitesComponent {
  // Variables vinculadas al HTML mediante [(ngModel)]
  evaluadoresInput: string = '';
  idComite: number | null = null;
  idSolicitudes: string = '';

  // Arreglo para la lista de comités
  comites: any[] = [];

  crearComite(): void {
    console.log('Creando comité con evaluadores:', this.evaluadoresInput);
    // Lógica para enviar al backend
  }

  asignarSolicitudes(): void {
    console.log(`Asignando solicitudes ${this.idSolicitudes} al comité ${this.idComite}`);
    // Lógica para enviar al backend
  }
}
# BecaNet Frontend — Sprint 3 (Angular)

Interfaz web en **Angular 18** (componentes standalone) que consume la API de
BecaNet (`BecaNet_Sprint3_Modulos.zip`) para los 4 módulos del Sprint 3:

| Pantalla | Ruta | Historias de usuario |
|---|---|---|
| Solicitudes | `/solicitudes` | US-007 (crear), US-009 (cancelar) |
| Documentación | `/documentos` | US-008 (cargar documento) |
| Comités Evaluadores | `/comites` | US-010 (crear), US-011 (asignar solicitudes) |
| Evaluaciones | `/evaluaciones` | US-012 (registrar evaluación) |

## Requisitos previos (gratuitos)

1. **Node.js 18 o superior** — https://nodejs.org
2. **Angular CLI** (se instala con `npm install -g @angular/cli` o se usa con `npx`)
3. Tener la **API de BecaNet corriendo** (proyecto `BecaNet_Sprint3_Modulos.zip`)

## Pasos para ejecutar

```bash
cd becanet-frontend
npm install
npm start
```

Esto abre la aplicación en `http://localhost:4200`.

## Conectar con la API

Antes de correr el frontend, abre `src/environments/environment.ts` y coloca el
puerto real en el que quedó corriendo la API de .NET (se ve en la consola al
ejecutar `dotnet run`, o en `launchSettings.json`):

```ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7080/api' // <-- ajustar este puerto
};
```

## Notas importantes

- Este proyecto **sí se generó, se instaló y se compiló dentro de este entorno**
  usando el Angular CLI real (`ng build` se ejecutó sin errores), a diferencia
  del backend en C# que no se pudo compilar por falta de acceso a NuGet. Aun así,
  se recomienda que el equipo corra `npm install` y `npm start` para probarlo
  contra la API real antes de darlo por terminado.
- Los formularios de "Comités" e "Evaluaciones" piden IDs numéricos (de
  estudiante, evaluador, convocatoria, etc.) porque los módulos de Sprint 1 y 2
  (login, listado de usuarios) todavía no están conectados a esta interfaz. Una
  vez esos módulos estén listos, se puede reemplazar esos campos de texto por
  selectores (dropdowns) que traigan los datos reales desde la API.
- CORS ya está habilitado en el backend para `http://localhost:4200` (ver
  `Program.cs` del proyecto de la API).

## Estructura del proyecto

```
src/app/
├── models/       → Interfaces TypeScript (igual a los DTOs de la API)
├── services/      → Un servicio HTTP por módulo (SolicitudService, etc.)
├── features/       → Un componente standalone por pantalla
│   ├── solicitudes/
│   ├── documentos/
│   ├── comites/
│   └── evaluaciones/
├── app.routes.ts    → Rutas de navegación
└── app.component.*  → Layout raíz con la barra de navegación
```

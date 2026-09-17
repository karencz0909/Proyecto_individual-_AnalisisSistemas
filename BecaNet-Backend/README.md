# BecaNet API — Sprint 3 (Módulos de Solicitudes, Documentación, Comités y Evaluaciones)

Código de los 4 módulos asignados en el Sprint 3 (21/09 – 02/10/2026), implementados
en **ASP.NET Core Web API (C#)**, siguiendo la **Arquitectura en Capas** ya definida
en el Anexo 5 (Controladores → Servicios → Repositorios → Entity Framework Core → SQL Server).

## Módulos incluidos

| Módulo | Historias de usuario | Archivos principales |
|---|---|---|
| Gestión de solicitudes de beca | US-007, US-009 | `Controllers/SolicitudesController.cs`, `Services/SolicitudService.cs` |
| Carga de documentación | US-008 | `Controllers/DocumentosController.cs`, `Services/DocumentoService.cs` |
| Gestión de comités evaluadores | US-010, US-011 | `Controllers/ComitesController.cs`, `Services/ComiteService.cs` |
| Registro de evaluaciones | US-012 | `Controllers/EvaluacionesController.cs`, `Services/EvaluacionService.cs` |

## Requisitos previos (todo gratuito)

1. **.NET 8 SDK** — descargar de https://dotnet.microsoft.com/download (Windows/Mac/Linux)
2. **SQL Server Express** + **SQL Server Management Studio (SSMS)** — https://www.microsoft.com/sql-server/sql-server-downloads
3. Un editor de código: Visual Studio Community (gratis) o Visual Studio Code

## Pasos para ejecutar el proyecto

### 1. Crear la base de datos

Abrir SSMS y ejecutar el script `BecaNet_Script_SQLServer.sql` que ya se entregó
anteriormente (crea la base `BecaNetDB` con las 11 tablas). Si prefieren que Entity
Framework la genere automáticamente en vez de correr el script a mano, pueden usar
las migraciones (paso 4 opcional).

### 2. Configurar la cadena de conexión

Abrir `BecaNet.Api/appsettings.json` y ajustar `ConnectionStrings:BecaNetDb` según el
nombre de su instancia de SQL Server (por defecto asume `localhost\SQLEXPRESS`).

### 3. Restaurar paquetes y ejecutar

```bash
cd BecaNet.Api
dotnet restore
dotnet run
```

La API quedará disponible en `https://localhost:{puerto}` y la documentación
interactiva (Swagger) en `https://localhost:{puerto}/swagger`, donde pueden probar
cada endpoint sin necesidad de tener el frontend en Angular listo todavía.

### 4. (Opcional) Generar la base de datos con migraciones de EF Core

Si prefieren que el propio código genere las tablas en vez de correr el script SQL:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InicialSprint3
dotnet ef database update
```

## Endpoints principales para probar en Swagger

- `POST /api/solicitudes` — crear una solicitud (US-007)
- `GET /api/solicitudes/estudiante/{idEstudiante}` — ver solicitudes de un estudiante
- `PUT /api/solicitudes/{id}/cancelar` — cancelar una solicitud (US-009)
- `POST /api/documentos/solicitud/{idSolicitud}` — subir un documento (form-data, campo `archivo`) (US-008)
- `GET /api/documentos/solicitud/{idSolicitud}` — ver documentos de una solicitud
- `POST /api/comites` — crear un comité con sus miembros (US-010)
- `POST /api/comites/asignar-solicitudes` — asignar solicitudes a un comité (US-011)
- `POST /api/evaluaciones` — registrar una evaluación (US-012)
- `GET /api/evaluaciones/solicitud/{idSolicitud}` — ver evaluaciones de una solicitud

## Nota importante para el equipo

Este código **no se pudo compilar ni ejecutar dentro de este entorno de IA** porque
no tiene acceso al SDK de .NET ni a NuGet (el gestor de paquetes de C#). Está escrito
siguiendo cuidadosamente la sintaxis y las buenas prácticas de ASP.NET Core 8 y
Entity Framework Core 8, pero **es indispensable que alguien del equipo lo abra en
Visual Studio o VS Code y lo compile (`dotnet build`)** antes de darlo por
terminado, por si aparece algún error de sintaxis menor que haya que corregir.

## Patrones y principios aplicados (útiles para la Justificación de Patrones)

- **Repository** (estructural): cada módulo tiene su repositorio (`ISolicitudRepository`,
  `IDocumentoRepository`, etc.) que aísla el acceso a datos de la lógica de negocio.
- **Dependency Injection** (incorporado en ASP.NET Core): los servicios y repositorios
  se inyectan por interfaz en los constructores, nunca se instancian con `new`.
- **DTO (Data Transfer Object)**: se usan clases separadas para entrada/salida de la
  API en vez de exponer las entidades de dominio directamente.
- **Single Responsibility (SOLID - S)**: cada capa (Controlador, Servicio, Repositorio)
  tiene una única responsabilidad.

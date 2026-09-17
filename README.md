# BecaNet - Sistema de Gestión de Becas

Sistema web para la administración de postulaciones, revisión de documentos, comités evaluadores y asignación de becas.

- Estructura del Repositorio

BecaNet-Backend/: API RESTful construida con .NET 8 , Entity Framework Core y SQL Server.
becanet-frontend/: Interfaz de usuario SPA desarrollada con Angular.

#- Guía de Inicio Rápido

 1. Base de Datos
1.1. Abrir SQL Server Management Studio (SSMS).
1.2. Ejecuta r el archivo de script unificado ubicado en:
BecaNet-Backend/BecaNet_Script_SQLServer.sql`

 2. Backend (.NET 8 API)
bash
cd BecaNet-Backend/BecaNet.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
URL API: http://localhost:5000

Swagger: http://localhost:5000/swagger

3. Frontend (Angular)
Bash
cd becanet-frontend
npm install
npm start
Aplicación Web: http://localhost:4200

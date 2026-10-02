using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Observers;
using BecaNet.Api.Repositories;
using BecaNet.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BecaNet.Api.Tests;

public class EvaluacionServiceTests
{
    private static BecaNetDbContext CrearContextoEnMemoria()
    {
        var opciones = new DbContextOptionsBuilder<BecaNetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BecaNetDbContext(opciones);
    }

    /// <summary>Crea una Solicitud con comité asignado, ya guardada en el contexto en memoria.</summary>
    private static async Task<Solicitud> SembrarSolicitudConComiteAsync(BecaNetDbContext context, int idComite, int totalMiembrosDelComite)
    {
        var solicitud = new Solicitud
        {
            Id = 1,
            IdEstudiante = 1,
            IdConvocatoria = 1,
            IdComite = idComite,
            Estado = EstadoSolicitud.EnProceso
        };
        context.Solicitudes.Add(solicitud);
        await context.SaveChangesAsync();
        return solicitud;
    }

    // ------------------------------------------------------------------
    // CASO 1: Un evaluador no puede evaluar dos veces la misma solicitud (US-012)
    // ------------------------------------------------------------------
    [Fact]
    public async Task RegistrarEvaluacionAsync_DebeFallar_SiElEvaluadorYaEvaluoEstaSolicitud()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var solicitud = await SembrarSolicitudConComiteAsync(context, idComite: 1, totalMiembrosDelComite: 2);

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        solicitudRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(solicitud);

        var comiteRepoMock = new Mock<IComiteRepository>();
        comiteRepoMock.Setup(r => r.EsMiembroAsync(1, 5)).ReturnsAsync(true);

        var evaluacionRepoMock = new Mock<IEvaluacionRepository>();
        evaluacionRepoMock
            .Setup(r => r.YaEvaluoAsync(1, 5))
            .ReturnsAsync(true); // <-- ya había evaluado antes

        var notificador = new SolicitudNotificador();
        var service = new EvaluacionService(
            evaluacionRepoMock.Object, comiteRepoMock.Object, solicitudRepoMock.Object, context, notificador);

        var dto = new RegistrarEvaluacionDTO
        {
            IdSolicitud = 1,
            IdEvaluador = 5,
            Puntaje = 85,
            Observaciones = "Cumple los requisitos"
        };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegistrarEvaluacionAsync(dto));

        Assert.Equal("Este evaluador ya registró una evaluación para esta solicitud.", excepcion.Message);
        evaluacionRepoMock.Verify(r => r.CrearAsync(It.IsAny<Evaluacion>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // CASO 2: Un evaluador que NO pertenece al comité asignado no puede evaluar
    // ------------------------------------------------------------------
    [Fact]
    public async Task RegistrarEvaluacionAsync_DebeFallar_SiEvaluadorNoPerteneceAlComiteAsignado()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var solicitud = await SembrarSolicitudConComiteAsync(context, idComite: 1, totalMiembrosDelComite: 2);

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        solicitudRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(solicitud);

        var comiteRepoMock = new Mock<IComiteRepository>();
        comiteRepoMock.Setup(r => r.EsMiembroAsync(1, 99)).ReturnsAsync(false); // <-- no es miembro

        var evaluacionRepoMock = new Mock<IEvaluacionRepository>();
        var notificador = new SolicitudNotificador();

        var service = new EvaluacionService(
            evaluacionRepoMock.Object, comiteRepoMock.Object, solicitudRepoMock.Object, context, notificador);

        var dto = new RegistrarEvaluacionDTO
        {
            IdSolicitud = 1,
            IdEvaluador = 99,
            Puntaje = 70,
            Observaciones = "Intento de evaluador externo"
        };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegistrarEvaluacionAsync(dto));

        Assert.Equal("El evaluador no pertenece al comité asignado a esta solicitud.", excepcion.Message);
    }

    // ------------------------------------------------------------------
    // CASO 3: Cuando el ÚLTIMO miembro del comité evalúa, la solicitud cambia a "Evaluada"
    // ------------------------------------------------------------------
    [Fact]
    public async Task RegistrarEvaluacionAsync_DebeCambiarEstadoAEvaluada_CuandoTodosLosMiembrosYaEvaluaron()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var solicitud = await SembrarSolicitudConComiteAsync(context, idComite: 1, totalMiembrosDelComite: 2);

        // Evaluación previa del primer miembro
        context.Evaluaciones.Add(new Evaluacion { Id = 1, IdSolicitud = 1, IdEvaluador = 5, Puntaje = 90 });
        await context.SaveChangesAsync();

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        solicitudRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(solicitud);

        var comiteRepoMock = new Mock<IComiteRepository>();
        comiteRepoMock.Setup(r => r.EsMiembroAsync(1, 6)).ReturnsAsync(true);
        comiteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(new ComiteEvaluador
        {
            Id = 1,
            Miembros = new List<ComiteMiembro> 
            { 
                new ComiteMiembro { IdEvaluador = 5 }, 
                new ComiteMiembro { IdEvaluador = 6 } 
            }
        });

        var evaluacionRepoMock = new Mock<IEvaluacionRepository>();
        evaluacionRepoMock.Setup(r => r.YaEvaluoAsync(1, 6)).ReturnsAsync(false);
        evaluacionRepoMock
            .Setup(r => r.CrearAsync(It.IsAny<Evaluacion>()))
            .ReturnsAsync((Evaluacion e) =>
            {
                context.Evaluaciones.Add(e);
                context.SaveChanges();
                return e;
            });

        // Observador de prueba
        var fueNotificado = false;
        var notificador = new SolicitudNotificador();
        notificador.Suscribir(new ObservadorDePrueba(() => fueNotificado = true));

        var service = new EvaluacionService(
            evaluacionRepoMock.Object, comiteRepoMock.Object, solicitudRepoMock.Object, context, notificador);

        var dto = new RegistrarEvaluacionDTO
        {
            IdSolicitud = 1,
            IdEvaluador = 6, // segundo y último miembro en evaluar
            Puntaje = 88,
            Observaciones = "Buen perfil académico"
        };

        // Act
        await service.RegistrarEvaluacionAsync(dto);

        // Assert
        var solicitudActualizada = await context.Solicitudes.FindAsync(1);
        Assert.Equal(EstadoSolicitud.Evaluada, solicitudActualizada!.Estado);
        Assert.True(fueNotificado, "El patrón Observer debió notificar el cambio de estado.");
    }

    private class ObservadorDePrueba : ISolicitudObserver
    {
        private readonly Action _alNotificar;
        public ObservadorDePrueba(Action alNotificar) => _alNotificar = alNotificar;
        public void OnCambioEstado(Solicitud solicitud, string estadoAnterior) => _alNotificar();
    }
}
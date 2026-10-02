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

public class SolicitudServiceTests
{
    /// <summary>
    /// Crea un BecaNetDbContext en memoria con un nombre de base de datos único
    /// </summary>
    private static BecaNetDbContext CrearContextoEnMemoria()
    {
        var opciones = new DbContextOptionsBuilder<BecaNetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BecaNetDbContext(opciones);
    }

    // ------------------------------------------------------------------
    // CASO 1: No se puede postular a una convocatoria que no esté ABIERTA
    // ------------------------------------------------------------------
    [Fact]
    public async Task CrearSolicitudAsync_DebeFallar_SiConvocatoriaNoEstaAbierta()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        context.Estudiantes.Add(new Estudiante
        {
            Id = 1,
            Nombre = "Maria Lopez",
            Correo = "maria@test.com",
            Contrasena = "hash",
            NivelAcademico = "Universitario"
        });

        context.Convocatorias.Add(new Convocatoria
        {
            Id = 1,
            Titulo = "Beca Universitaria 2026",
            FechaApertura = DateTime.Now,
            FechaCierre = DateTime.Now.AddDays(30),
            IdCoordinador = 1,
            Estado = EstadoConvocatoria.Borrador // <-- todavía NO está abierta
        });

        await context.SaveChangesAsync();

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        var notificador = new SolicitudNotificador(); // sin observadores suscritos, no afecta la prueba

        var service = new SolicitudService(solicitudRepoMock.Object, context, notificador);

        var dto = new CrearSolicitudDTO { IdEstudiante = 1, IdConvocatoria = 1 };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearSolicitudAsync(dto));

        Assert.Equal("Solo se puede postular a convocatorias en estado ABIERTA.", excepcion.Message);

        // Verificamos que, como falló antes, el repositorio NUNCA llegó a intentar crear la solicitud
        solicitudRepoMock.Verify(r => r.CrearAsync(It.IsAny<Solicitud>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // CASO 2: No se puede duplicar una solicitud activa en la misma convocatoria
    // ------------------------------------------------------------------
    [Fact]
    public async Task CrearSolicitudAsync_DebeFallar_SiYaExisteSolicitudActivaParaLaConvocatoria()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        context.Estudiantes.Add(new Estudiante
        {
            Id = 1,
            Nombre = "Maria Lopez",
            Correo = "maria@test.com",
            Contrasena = "hash",
            NivelAcademico = "Universitario"
        });

        context.Convocatorias.Add(new Convocatoria
        {
            Id = 1,
            Titulo = "Beca Universitaria 2026",
            FechaApertura = DateTime.Now,
            FechaCierre = DateTime.Now.AddDays(30),
            IdCoordinador = 1,
            Estado = EstadoConvocatoria.Abierta // esta vez SÍ está abierta
        });

        await context.SaveChangesAsync();

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        // Simulamos que el repositorio dice que YA existe una solicitud activa
        solicitudRepoMock
            .Setup(r => r.ExisteSolicitudActivaAsync(1, 1))
            .ReturnsAsync(true);

        var notificador = new SolicitudNotificador();
        var service = new SolicitudService(solicitudRepoMock.Object, context, notificador);

        var dto = new CrearSolicitudDTO { IdEstudiante = 1, IdConvocatoria = 1 };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearSolicitudAsync(dto));

        Assert.Equal("Ya existe una solicitud activa para esta convocatoria.", excepcion.Message);
        solicitudRepoMock.Verify(r => r.CrearAsync(It.IsAny<Solicitud>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // CASO 3: No se puede rechazar una solicitud sin indicar un motivo
    // ------------------------------------------------------------------
    [Fact]
    public async Task ResolverAsync_DebeFallar_SiRechazaSinMotivo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        var solicitudExistente = new Solicitud
        {
            Id = 10,
            IdEstudiante = 1,
            IdConvocatoria = 1,
            Estado = EstadoSolicitud.Evaluada, // ya fue evaluada por el comité
            Documentos = new List<Documento>()
        };

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        solicitudRepoMock
            .Setup(r => r.ObtenerPorIdAsync(10))
            .ReturnsAsync(solicitudExistente);

        var notificador = new SolicitudNotificador();
        var service = new SolicitudService(solicitudRepoMock.Object, context, notificador);

        // Intentamos RECHAZAR (Aprobar = false) sin mandar motivo
        var dto = new ResolverSolicitudDTO { Aprobar = false, Motivo = null };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ResolverAsync(10, dto));

        Assert.Equal("Debes indicar un motivo para rechazar la solicitud.", excepcion.Message);

        // El estado NUNCA debió haberse actualizado en el repositorio
        solicitudRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Solicitud>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // BONUS - CASO POSITIVO: Aprobar SÍ funciona cuando todo es correcto
    // (para que veas también cómo se prueba el "camino feliz", no solo errores)
    // ------------------------------------------------------------------
    [Fact]
    public async Task ResolverAsync_DebeAprobar_CuandoLaSolicitudEstaEvaluada()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        var solicitudExistente = new Solicitud
        {
            Id = 20,
            IdEstudiante = 1,
            IdConvocatoria = 1,
            Estado = EstadoSolicitud.Evaluada,
            Documentos = new List<Documento>()
        };

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        solicitudRepoMock
            .Setup(r => r.ObtenerPorIdAsync(20))
            .ReturnsAsync(solicitudExistente);

        var notificador = new SolicitudNotificador();
        var service = new SolicitudService(solicitudRepoMock.Object, context, notificador);

        var dto = new ResolverSolicitudDTO { Aprobar = true, Motivo = "Cumple todos los requisitos" };

        // Act
        var resultado = await service.ResolverAsync(20, dto);

        // Assert
        Assert.Equal(EstadoSolicitud.Aprobada, resultado.Estado);
        Assert.NotNull(resultado.FechaResolucion);
        solicitudRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Solicitud>()), Times.Once);
    }
}
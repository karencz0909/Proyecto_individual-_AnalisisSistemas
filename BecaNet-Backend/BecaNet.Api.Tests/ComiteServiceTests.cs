using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;
using BecaNet.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BecaNet.Api.Tests;

public class ComiteServiceTests
{
    private static BecaNetDbContext CrearContextoEnMemoria()
    {
        var opciones = new DbContextOptionsBuilder<BecaNetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BecaNetDbContext(opciones);
    }

    // ------------------------------------------------------------------
    // CASO 1: Un comité no se puede crear con menos de 2 miembros (US-010)
    // ------------------------------------------------------------------
    [Fact]
    public async Task CrearComiteAsync_DebeFallar_SiTieneMenosDeDosMiembros()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        var comiteRepoMock = new Mock<IComiteRepository>();
        var solicitudRepoMock = new Mock<ISolicitudRepository>();

        var service = new ComiteService(comiteRepoMock.Object, solicitudRepoMock.Object, context);

        var dto = new CrearComiteDTO
        {
            Nombre = "Comité Becas 2026",
            IdsEvaluadores = new List<int> { 1 } // <-- solo 1 miembro, se requieren al menos 2
        };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearComiteAsync(dto));

        Assert.Equal("Un comité debe tener al menos 2 miembros asignados.", excepcion.Message);

        comiteRepoMock.Verify(r => r.CrearAsync(It.IsAny<ComiteEvaluador>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // CASO 2: Un comité SÍ se puede crear con 2 o más evaluadores válidos
    //         (camino feliz, complementa el caso 1)
    // ------------------------------------------------------------------
    [Fact]
    public async Task CrearComiteAsync_DebeCrear_CuandoTieneAlMenosDosMiembrosValidos()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        var evaluadoresExistentes = new List<EvaluadorComite>
        {
            new() { Id = 1, Nombre = "Carlos Perez", Correo = "carlos@test.com", Contrasena = "hash" },
            new() { Id = 2, Nombre = "Ana Gomez", Correo = "ana@test.com", Contrasena = "hash" }
        };

        var comiteRepoMock = new Mock<IComiteRepository>();
        comiteRepoMock
            .Setup(r => r.ObtenerEvaluadoresPorIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(evaluadoresExistentes);

        comiteRepoMock
            .Setup(r => r.CrearAsync(It.IsAny<ComiteEvaluador>()))
            .ReturnsAsync((ComiteEvaluador c) => c);

        comiteRepoMock
            .Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComiteEvaluador
            {
                Id = 1,
                Nombre = "Comité Becas 2026",
                Miembros = new List<ComiteMiembro>
                {
                    new() { IdEvaluador = 1, Evaluador = evaluadoresExistentes[0] },
                    new() { IdEvaluador = 2, Evaluador = evaluadoresExistentes[1] }
                }
            });

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        var service = new ComiteService(comiteRepoMock.Object, solicitudRepoMock.Object, context);

        var dto = new CrearComiteDTO
        {
            Nombre = "Comité Becas 2026",
            IdsEvaluadores = new List<int> { 1, 2 }
        };

        // Act
        var resultado = await service.CrearComiteAsync(dto);

        // Assert
        Assert.Equal(2, resultado.Miembros.Count);
        comiteRepoMock.Verify(r => r.CrearAsync(It.IsAny<ComiteEvaluador>()), Times.Once);
    }

    // ------------------------------------------------------------------
    // CASO 3: No se puede asignar una solicitud sin documentación cargada (US-011)
    // ------------------------------------------------------------------
    [Fact]
    public async Task AsignarSolicitudesAsync_DebeFallar_SiSolicitudNoTieneDocumentos()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        var comiteRepoMock = new Mock<IComiteRepository>();
        comiteRepoMock
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(new ComiteEvaluador { Id = 1, Nombre = "Comité Becas 2026" });

        var solicitudSinDocumentos = new Solicitud
        {
            Id = 10,
            Estado = EstadoSolicitud.EnProceso,
            Documentos = new List<Documento>() // <-- sin documentos
        };

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        solicitudRepoMock
            .Setup(r => r.ObtenerPorIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Solicitud> { solicitudSinDocumentos });

        var service = new ComiteService(comiteRepoMock.Object, solicitudRepoMock.Object, context);

        var dto = new AsignarSolicitudesDTO { IdComite = 1, IdsSolicitudes = new List<int> { 10 } };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AsignarSolicitudesAsync(dto));

        Assert.Equal("La solicitud 10 no tiene documentación cargada.", excepcion.Message);
        solicitudRepoMock.Verify(r => r.ActualizarAsync(It.IsAny<Solicitud>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // CASO 4: No se puede asignar una solicitud que no esté EN_PROCESO
    //         (por ejemplo, una ya cancelada)
    // ------------------------------------------------------------------
    [Fact]
    public async Task AsignarSolicitudesAsync_DebeFallar_SiSolicitudNoEstaEnProceso()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();

        var comiteRepoMock = new Mock<IComiteRepository>();
        comiteRepoMock
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(new ComiteEvaluador { Id = 1, Nombre = "Comité Becas 2026" });

        var solicitudCancelada = new Solicitud
        {
            Id = 11,
            Estado = EstadoSolicitud.Cancelada, // <-- ya no está en proceso
            Documentos = new List<Documento> { new() { Id = 1 } }
        };

        var solicitudRepoMock = new Mock<ISolicitudRepository>();
        solicitudRepoMock
            .Setup(r => r.ObtenerPorIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Solicitud> { solicitudCancelada });

        var service = new ComiteService(comiteRepoMock.Object, solicitudRepoMock.Object, context);

        var dto = new AsignarSolicitudesDTO { IdComite = 1, IdsSolicitudes = new List<int> { 11 } };

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AsignarSolicitudesAsync(dto));

        Assert.Equal("La solicitud 11 no está en estado EN_PROCESO.", excepcion.Message);
    }
}
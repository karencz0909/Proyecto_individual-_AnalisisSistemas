using BecaNet.Api.Data;
using BecaNet.Api.Models;
using BecaNet.Api.Repositories;
using BecaNet.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BecaNet.Api.Tests;

public class DocumentoServiceTests
{
    private static BecaNetDbContext CrearContextoEnMemoria()
    {
        var opciones = new DbContextOptionsBuilder<BecaNetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BecaNetDbContext(opciones);
    }

    /// <summary>
    /// Crea un IFormFile falso en memoria, con el nombre y tamaño que la prueba necesite,
    /// sin tener que usar un archivo real del disco.
    /// </summary>
    private static IFormFile CrearArchivoFalso(string nombreArchivo, int tamanoEnBytes)
    {
        var contenido = new byte[tamanoEnBytes];
        var stream = new MemoryStream(contenido);
        return new FormFile(stream, 0, contenido.Length, "archivo", nombreArchivo);
    }

    /// <summary>IWebHostEnvironment falso que apunta a una carpeta temporal del sistema,
    /// para que el servicio pueda "guardar" el archivo sin tocar wwwroot real.</summary>
    private static Mock<IWebHostEnvironment> CrearEntornoFalso()
    {
        var entornoMock = new Mock<IWebHostEnvironment>();
        entornoMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        return entornoMock;
    }

    // ------------------------------------------------------------------
    // CASO 1: Rechazar un formato de archivo no permitido (solo PDF/JPG/PNG)
    // ------------------------------------------------------------------
    [Fact]
    public async Task CargarDocumentoAsync_DebeFallar_SiElFormatoNoEstaPermitido()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        context.Solicitudes.Add(new Solicitud { Id = 1, IdEstudiante = 1, IdConvocatoria = 1 });
        await context.SaveChangesAsync();

        var documentoRepoMock = new Mock<IDocumentoRepository>();
        var entornoMock = CrearEntornoFalso();

        var service = new DocumentoService(documentoRepoMock.Object, context, entornoMock.Object);

        var archivoNoPermitido = CrearArchivoFalso("certificado.docx", 1024); // <-- .docx no está permitido

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CargarDocumentoAsync(1, archivoNoPermitido));

        Assert.Equal("Formato de archivo no permitido. Solo se aceptan PDF, JPG y PNG.", excepcion.Message);
        documentoRepoMock.Verify(r => r.CrearAsync(It.IsAny<Documento>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // CASO 2: Rechazar un archivo que supera el tamaño máximo (5 MB)
    // ------------------------------------------------------------------
    [Fact]
    public async Task CargarDocumentoAsync_DebeFallar_SiElArchivoSuperaElTamanoMaximo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        context.Solicitudes.Add(new Solicitud { Id = 1, IdEstudiante = 1, IdConvocatoria = 1 });
        await context.SaveChangesAsync();

        var documentoRepoMock = new Mock<IDocumentoRepository>();
        var entornoMock = CrearEntornoFalso();

        var service = new DocumentoService(documentoRepoMock.Object, context, entornoMock.Object);

        // 6 MB, por encima del límite de 5 MB (TiposArchivoPermitidos.TamanoMaximoBytes)
        var archivoMuyPesado = CrearArchivoFalso("certificado.pdf", 6 * 1024 * 1024);

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CargarDocumentoAsync(1, archivoMuyPesado));

        Assert.Equal("El archivo supera el tamaño máximo permitido (5 MB).", excepcion.Message);
        documentoRepoMock.Verify(r => r.CrearAsync(It.IsAny<Documento>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // CASO 3: Rechazar la carga si la solicitud no existe
    // ------------------------------------------------------------------
    [Fact]
    public async Task CargarDocumentoAsync_DebeFallar_SiLaSolicitudNoExiste()
    {
        // Arrange
        using var context = CrearContextoEnMemoria(); // sin ninguna Solicitud sembrada

        var documentoRepoMock = new Mock<IDocumentoRepository>();
        var entornoMock = CrearEntornoFalso();

        var service = new DocumentoService(documentoRepoMock.Object, context, entornoMock.Object);
        var archivoValido = CrearArchivoFalso("certificado.pdf", 1024);

        // Act + Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CargarDocumentoAsync(999, archivoValido)); // id que no existe

        Assert.Equal("La solicitud no existe.", excepcion.Message);
    }

    // ------------------------------------------------------------------
    // CASO 4 (camino feliz): un archivo PDF válido y dentro del límite SÍ se guarda
    // ------------------------------------------------------------------
    [Fact]
    public async Task CargarDocumentoAsync_DebeGuardar_CuandoElArchivoEsValido()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        context.Solicitudes.Add(new Solicitud { Id = 1, IdEstudiante = 1, IdConvocatoria = 1 });
        await context.SaveChangesAsync();

        var documentoRepoMock = new Mock<IDocumentoRepository>();
        documentoRepoMock
            .Setup(r => r.CrearAsync(It.IsAny<Documento>()))
            .ReturnsAsync((Documento d) => d);

        var entornoMock = CrearEntornoFalso();
        var service = new DocumentoService(documentoRepoMock.Object, context, entornoMock.Object);

        var archivoValido = CrearArchivoFalso("certificado.pdf", 1024); // 1 KB, formato permitido

        // Act
        var resultado = await service.CargarDocumentoAsync(1, archivoValido);

        // Assert
        Assert.Equal("PDF", resultado.TipoArchivo);
        Assert.Equal("certificado.pdf", resultado.NombreArchivo);
        documentoRepoMock.Verify(r => r.CrearAsync(It.IsAny<Documento>()), Times.Once);
    }
}
using BecaNet.Api.Data;
using BecaNet.Api.DTOs;
using BecaNet.Api.Models;
using BecaNet.Api.Observers;
using BecaNet.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BecaNet.Api.Tests;

public class AsignacionServiceTests
{
    private static BecaNetDbContext CrearContextoEnMemoria()
    {
        var opciones = new DbContextOptionsBuilder<BecaNetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BecaNetDbContext(opciones);
    }

    private static void SembrarEvaluadores(BecaNetDbContext context, int cantidad)
    {
        for (var i = 1; i <= cantidad; i++)
        {
            context.Evaluadores.Add(new EvaluadorComite
            {
                Id = i,
                Nombre = $"Evaluador {i}",
                Correo = $"eval{i}@test.com",
                Contrasena = "hash"
            });
        }
        context.SaveChanges();
    }

    private static void SembrarSolicitudes(BecaNetDbContext context, int cantidad, bool conCertificacion = true)
    {
        for (var i = 1; i <= cantidad; i++)
        {
            var solicitud = new Solicitud
            {
                Id = i,
                IdEstudiante = 100 + i,
                IdConvocatoria = 1,
                Estado = EstadoSolicitud.EnProceso,
                FechaCreacion = DateTime.Now.AddMinutes(i)
            };

            if (conCertificacion)
            {
                solicitud.Documentos.Add(new Documento
                {
                    Id = i,
                    IdSolicitud = i,
                    NombreArchivo = "certificacion.pdf",
                    TipoArchivo = "PDF",
                    TipoDocumento = TiposDocumento.CertificacionEstudios,
                    UrlArchivo = "/uploads/certificacion.pdf"
                });
            }

            context.Solicitudes.Add(solicitud);
        }
        context.SaveChanges();
    }

    // ------------------------------------------------------------------
    // CASO 1: 10 solicitudes entre 5 evaluadores -> 2 para cada uno
    // ------------------------------------------------------------------
    [Fact]
    public async Task DistribuirAsync_DebeRepartirEquitativamente_DiezSolicitudesEntreCincoEvaluadores()
    {
        using var context = CrearContextoEnMemoria();
        SembrarEvaluadores(context, 5);
        SembrarSolicitudes(context, 10);
        var service = new AsignacionService(context, new SolicitudNotificador());

        var resultado = await service.DistribuirAsync(new DistribuirDTO());

        Assert.Equal(10, resultado.TotalAsignadas);
        Assert.Equal(5, resultado.PorEvaluador.Count);
        Assert.All(resultado.PorEvaluador, e => Assert.Equal(2, e.Asignadas));
        Assert.All(context.Solicitudes.ToList(), s => Assert.Equal(EstadosRevision.EnRevision, s.EstadoRevision));
    }

    // ------------------------------------------------------------------
    // CASO 2: sin certificación de estudios no se asigna nada
    // ------------------------------------------------------------------
    [Fact]
    public async Task DistribuirAsync_DebeOmitirSolicitudesSinCertificacionDeEstudios()
    {
        using var context = CrearContextoEnMemoria();
        SembrarEvaluadores(context, 2);
        SembrarSolicitudes(context, 3, conCertificacion: false);
        var service = new AsignacionService(context, new SolicitudNotificador());

        var resultado = await service.DistribuirAsync(new DistribuirDTO());

        Assert.Equal(0, resultado.TotalAsignadas);
        Assert.Equal(3, resultado.OmitidasSinCertificacion);
    }

    // ------------------------------------------------------------------
    // CASO 3: al revisar, la solicitud pasa a EVALUADA y el Observer avisa
    // ------------------------------------------------------------------
    [Fact]
    public async Task RevisarAsync_DebeMarcarRevisadoYNotificarCambioDeEstado()
    {
        using var context = CrearContextoEnMemoria();
        SembrarEvaluadores(context, 1);
        SembrarSolicitudes(context, 1);

        var solicitud = context.Solicitudes.First();
        solicitud.IdEvaluadorAsignado = 1;
        solicitud.EstadoRevision = EstadosRevision.EnRevision;
        context.SaveChanges();

        var fueNotificado = false;
        var notificador = new SolicitudNotificador();
        notificador.Suscribir(new ObservadorDePrueba(() => fueNotificado = true));
        var service = new AsignacionService(context, notificador);

        var dto = new RevisarSolicitudDTO { Puntaje = 90, Observaciones = "Excelente perfil académico" };
        var resultado = await service.RevisarAsync(1, 1, dto);

        Assert.Equal(EstadosRevision.Revisado, resultado.EstadoRevision);
        Assert.Equal(EstadoSolicitud.Evaluada, resultado.EstadoSolicitud);
        Assert.Equal(90m, resultado.Puntaje!.Value);
        Assert.True(fueNotificado, "El patrón Observer debió notificar el cambio de estado.");
    }

    // ------------------------------------------------------------------
    // CASO 4: un evaluador no puede calificar una solicitud que no es suya
    // ------------------------------------------------------------------
    [Fact]
    public async Task RevisarAsync_DebeFallar_SiLaSolicitudNoEstaAsignadaAlEvaluador()
    {
        using var context = CrearContextoEnMemoria();
        SembrarEvaluadores(context, 2);
        SembrarSolicitudes(context, 1);

        var solicitud = context.Solicitudes.First();
        solicitud.IdEvaluadorAsignado = 1; // es del evaluador 1
        solicitud.EstadoRevision = EstadosRevision.EnRevision;
        context.SaveChanges();

        var service = new AsignacionService(context, new SolicitudNotificador());
        var dto = new RevisarSolicitudDTO { Puntaje = 80, Observaciones = "Intento ajeno" };

        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RevisarAsync(2, 1, dto)); // lo intenta el evaluador 2

        Assert.Equal("Esta solicitud no está asignada a ti.", excepcion.Message);
    }

    private class ObservadorDePrueba : ISolicitudObserver
    {
        private readonly Action _alNotificar;
        public ObservadorDePrueba(Action alNotificar) => _alNotificar = alNotificar;
        public void OnCambioEstado(Solicitud solicitud, string estadoAnterior) => _alNotificar();
    }
}

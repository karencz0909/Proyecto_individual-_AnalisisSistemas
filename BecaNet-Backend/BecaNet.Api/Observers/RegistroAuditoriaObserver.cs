using BecaNet.Api.Models;

namespace BecaNet.Api.Observers;

/// Observado  #2: deja constancia en el log de auditoría 


public class RegistroAuditoriaObserver : ISolicitudObserver
{
    private readonly ILogger<RegistroAuditoriaObserver> _logger;

    public RegistroAuditoriaObserver(ILogger<RegistroAuditoriaObserver> logger)
    {
        _logger = logger;
    }

    public void OnCambioEstado(Solicitud solicitud, string estadoAnterior)
    {
        _logger.LogInformation(
            "🗂 AUDITORÍA: Solicitud #{IdSolicitud} | {Anterior} → {Nuevo} | {Fecha}",
            solicitud.Id, estadoAnterior, solicitud.Estado, DateTime.Now);
    }
}
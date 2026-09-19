using BecaNet.Api.Models;

namespace BecaNet.Api.Observers;


/// Observador #1: simula el envío de una notificación al estudiante

public class NotificacionEstudianteObserver : ISolicitudObserver
{
    private readonly ILogger<NotificacionEstudianteObserver> _logger;

    public NotificacionEstudianteObserver(ILogger<NotificacionEstudianteObserver> logger)
    {
        _logger = logger;
    }

    public void OnCambioEstado(Solicitud solicitud, string estadoAnterior)
    {
        // En una versión futura, aquí se integraría un servicio real de correo o
        // notificaciones push. Por ahora se deja registrado en el log del sistema.
        _logger.LogInformation(
            "📩 Notificación al estudiante #{IdEstudiante}: su solicitud #{IdSolicitud} cambió de {Anterior} a {Nuevo}.",
            solicitud.IdEstudiante, solicitud.Id, estadoAnterior, solicitud.Estado);
    }
}
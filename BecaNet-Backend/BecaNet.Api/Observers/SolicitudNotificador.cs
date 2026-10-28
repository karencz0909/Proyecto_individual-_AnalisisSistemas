using BecaNet.Api.Models;

namespace BecaNet.Api.Observers;

/// "Sujeto" 
public class SolicitudNotificador
{
    private readonly ILogger<SolicitudNotificador>? _logger;
    private readonly List<ISolicitudObserver> _observadores = new();

    public SolicitudNotificador(ILogger<SolicitudNotificador>? logger = null) => _logger = logger;

    public void Suscribir(ISolicitudObserver observador)
    {
        _observadores.Add(observador);
    }

    public void NotificarCambioEstado(Solicitud solicitud, string estadoAnterior)
    {
        foreach (var observador in _observadores)
            try { observador.OnCambioEstado(solicitud, estadoAnterior); }
            catch (Exception ex) { _logger?.LogError(ex, "Falló un canal de notificación para solicitud {IdSolicitud}; el estado ya fue guardado.", solicitud.Id); }
    }
}

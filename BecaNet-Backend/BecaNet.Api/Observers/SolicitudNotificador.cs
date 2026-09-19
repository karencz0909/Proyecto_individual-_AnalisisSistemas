using BecaNet.Api.Models;

namespace BecaNet.Api.Observers;

/// "Sujeto" 
public class SolicitudNotificador
{
    private readonly List<ISolicitudObserver> _observadores = new();

    public void Suscribir(ISolicitudObserver observador)
    {
        _observadores.Add(observador);
    }

    public void NotificarCambioEstado(Solicitud solicitud, string estadoAnterior)
    {
        foreach (var observador in _observadores)
        {
            observador.OnCambioEstado(solicitud, estadoAnterior);
        }
    }
}
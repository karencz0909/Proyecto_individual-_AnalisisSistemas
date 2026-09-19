using BecaNet.Api.Models;

namespace BecaNet.Api.Observers;


/// cPatron Observer

public interface ISolicitudObserver
{
    void OnCambioEstado(Solicitud solicitud, string estadoAnterior);
}
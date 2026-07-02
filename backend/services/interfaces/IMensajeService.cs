using services.dtos;

namespace services.interfaces;

public interface IMensajeService
{
    Task<MensajeResponse> SendMensajeAsync(
        Guid employerUserId,
        SendMensajeRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MensajeResponse>> GetMisBandejaEntradaAsync(
        Guid candidateUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MensajeResponse>> GetConversacionForEmployerAsync(
        Guid employerUserId,
        Guid postulacionId,
        CancellationToken cancellationToken);
}

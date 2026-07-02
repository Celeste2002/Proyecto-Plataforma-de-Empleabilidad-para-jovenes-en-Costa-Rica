using domain.entities;

namespace services.interfaces;

public interface INotificacionRepository
{
    Task SaveAsync(Notificacion notificacion, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Notificacion>> GetByEmployerProfileIdAsync(
        Guid employerProfileId,
        Guid? vacanteId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Notificacion>> GetByCandidateProfileIdAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken);

    Task MarkAsReadAsync(
        Guid notificacionId,
        Guid employerProfileId,
        CancellationToken cancellationToken);

    Task MarkEmployerVacanteAsReadAsync(
        Guid employerProfileId,
        Guid? vacanteId,
        CancellationToken cancellationToken);

    Task MarkCandidateNotificationsAsReadAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken);

    Task<int> GetUnreadCountAsync(
        Guid employerProfileId,
        CancellationToken cancellationToken);

    Task<int> GetCandidateUnreadCountAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken);
}

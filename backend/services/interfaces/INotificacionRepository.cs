using domain.entities;

namespace services.interfaces;

public interface INotificacionRepository
{
    Task SaveAsync(Notificacion notificacion, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Notificacion>> GetByEmployerProfileIdAsync(
        Guid employerProfileId,
        Guid? vacanteId,
        CancellationToken cancellationToken);

    Task MarkAsReadAsync(
        Guid notificacionId,
        Guid employerProfileId,
        CancellationToken cancellationToken);

    Task<int> GetUnreadCountAsync(
        Guid employerProfileId,
        CancellationToken cancellationToken);
}

using domain.entities;

namespace services.interfaces;

public interface IMensajeRepository
{
    Task SaveAsync(Mensaje mensaje, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Mensaje>> GetByPostulacionIdAsync(Guid postulacionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Mensaje>> GetByRecipientCandidateProfileIdAsync(Guid candidateProfileId, CancellationToken cancellationToken);
}

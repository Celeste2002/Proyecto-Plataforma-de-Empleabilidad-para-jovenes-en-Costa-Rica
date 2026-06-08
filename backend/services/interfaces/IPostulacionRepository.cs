using domain.entities;

namespace services.interfaces;

public interface IPostulacionRepository
{
    Task SaveAsync(Postulacion postulacion, CancellationToken cancellationToken);
    Task<bool> ExistsByVacanteAndCandidateAsync(Guid vacanteId, Guid candidateProfileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Postulacion>> GetByCandidateProfileIdAsync(Guid candidateProfileId, CancellationToken cancellationToken);
}

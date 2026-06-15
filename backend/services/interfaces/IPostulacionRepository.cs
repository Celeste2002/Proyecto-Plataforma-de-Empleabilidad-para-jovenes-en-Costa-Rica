using domain.entities;

namespace services.interfaces;

public interface IPostulacionRepository
{
    Task SaveAsync(Postulacion postulacion, CancellationToken cancellationToken);
    Task<bool> ExistsByVacanteAndCandidateAsync(Guid vacanteId, Guid candidateProfileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Postulacion>> GetByCandidateProfileIdAsync(Guid candidateProfileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Postulacion>> GetByVacanteIdAsync(Guid vacanteId, CancellationToken cancellationToken);
    Task<Postulacion?> FindByIdAsync(Guid postulacionId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid postulacionId, string newStatus, DateTime updatedAtUtc, CancellationToken cancellationToken);
}

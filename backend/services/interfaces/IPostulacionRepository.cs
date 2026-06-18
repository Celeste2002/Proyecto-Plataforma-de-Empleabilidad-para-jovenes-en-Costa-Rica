using domain.entities;

namespace services.interfaces;

public interface IPostulacionRepository
{
    Task SaveAsync(Postulacion postulacion, CancellationToken cancellationToken);
    Task<bool> ExistsByVacanteAndCandidateAsync(Guid vacanteId, Guid candidateProfileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Postulacion>> GetByCandidateProfileIdAsync(Guid candidateProfileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Postulacion>> GetByVacanteForEmployerAsync(Guid vacanteId, Guid employerProfileId, CancellationToken cancellationToken);
    Task<Postulacion?> FindByIdForEmployerAsync(Guid id, Guid employerProfileId, CancellationToken cancellationToken);
    Task<bool> UpdateStatusForEmployerAsync(Guid id, Guid employerProfileId, string status, DateTime updatedAtUtc, CancellationToken cancellationToken);
}

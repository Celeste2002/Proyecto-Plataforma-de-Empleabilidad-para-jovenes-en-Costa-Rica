using domain.entities;

namespace services.interfaces;

public interface ISugerenciaPostulacionRepository
{
    Task SaveAsync(SugerenciaPostulacion sugerencia, CancellationToken cancellationToken);

    Task<bool> ExistsByVacanteAndCandidateAsync(Guid vacanteId, Guid candidateProfileId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SugerenciaPostulacion>> GetByCandidateProfileIdAsync(
        Guid candidateProfileId, CancellationToken cancellationToken);
}

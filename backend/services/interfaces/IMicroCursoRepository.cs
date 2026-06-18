using domain.entities;

namespace services.interfaces;

public interface IMicroCursoRepository
{
    Task<IReadOnlyCollection<MicroCurso>> GetValidatedAsync(string? area, CancellationToken cancellationToken);
    Task<MicroCurso?> FindValidatedByIdAsync(Guid id, CancellationToken cancellationToken);
}


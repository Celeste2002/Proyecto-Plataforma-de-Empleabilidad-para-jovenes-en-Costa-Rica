using domain.entities;

namespace services.interfaces;

public interface IVacanteRepository
{
    Task<IReadOnlyCollection<Vacante>> GetActiveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Vacante>> GetByEmployerProfileIdAsync(Guid employerProfileId, CancellationToken cancellationToken);
    Task<Vacante?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(Vacante vacante, CancellationToken cancellationToken);
    Task<bool> UpdateEditableFieldsAsync(
        Guid id,
        Guid employerProfileId,
        string? description,
        string? requirements,
        string? salaryRange,
        CancellationToken cancellationToken);
}

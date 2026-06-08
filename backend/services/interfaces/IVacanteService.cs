using services.dtos;

namespace services.interfaces;

public interface IVacanteService
{
    Task<IReadOnlyCollection<VacanteResponse>> GetActiveVacantesAsync(CancellationToken cancellationToken);
    Task ApplyAsync(Guid candidateUserId, ApplyToVacanteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PostulacionResponse>> GetMyPostulacionesAsync(Guid candidateUserId, CancellationToken cancellationToken);
}

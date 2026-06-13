using services.dtos;

namespace services.interfaces;

public interface IVacanteService
{
    Task<IReadOnlyCollection<VacanteResponse>> GetActiveVacantesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VacanteResponse>> GetMyVacantesAsync(Guid employerUserId, CancellationToken cancellationToken);
    Task<VacanteResponse> CreateVacanteAsync(Guid employerUserId, CreateVacanteRequest request, CancellationToken cancellationToken);
    Task ApplyAsync(Guid candidateUserId, ApplyToVacanteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PostulacionResponse>> GetMyPostulacionesAsync(Guid candidateUserId, CancellationToken cancellationToken);
}

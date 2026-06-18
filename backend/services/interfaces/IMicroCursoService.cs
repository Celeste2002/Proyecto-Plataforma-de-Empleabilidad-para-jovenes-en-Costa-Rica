using services.dtos;

namespace services.interfaces;

public interface IMicroCursoService
{
    Task<IReadOnlyCollection<MicroCursoResponse>> GetCatalogoAsync(string? area, CancellationToken cancellationToken);
    Task<MicroCursoResponse> GetDetalleAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MicroCursoResponse>> GetRecomendadosAsync(Guid candidateUserId, CancellationToken cancellationToken);
}


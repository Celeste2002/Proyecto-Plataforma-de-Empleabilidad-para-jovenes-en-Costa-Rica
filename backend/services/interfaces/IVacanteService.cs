using services.dtos;

namespace services.interfaces;

public interface IVacanteService
{
    Task<IReadOnlyCollection<VacanteResponse>> GetActiveVacantesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VacanteResponse>> GetMyVacantesAsync(Guid employerUserId, CancellationToken cancellationToken);
    Task<VacanteResponse> CreateVacanteAsync(Guid employerUserId, CreateVacanteRequest request, CancellationToken cancellationToken);
    Task<VacanteResponse> UpdateVacanteAsync(Guid employerUserId, Guid vacanteId, UpdateVacanteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmployerPostulacionResponse>> GetPostulacionesByVacanteAsync(Guid employerUserId, Guid vacanteId, CancellationToken cancellationToken);
    Task<EmployerPostulacionResponse> RequestInterviewAsync(Guid employerUserId, Guid postulacionId, CancellationToken cancellationToken);
    Task ApplyAsync(Guid candidateUserId, ApplyToVacanteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PostulacionResponse>> GetMyPostulacionesAsync(Guid candidateUserId, CancellationToken cancellationToken);
}

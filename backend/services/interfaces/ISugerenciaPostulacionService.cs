using services.dtos;

namespace services.interfaces;

public interface ISugerenciaPostulacionService
{
    Task<SugerenciaPostulacionResponse> CreateAsync(
        Guid employerUserId,
        CreateSugerenciaPostulacionRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SugerenciaRecibidaResponse>> GetRecibidasByCandidateAsync(
        Guid candidateUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetAppliedVacanteIdsAsync(
        Guid employerUserId,
        Guid candidateProfileId,
        CancellationToken cancellationToken);
}

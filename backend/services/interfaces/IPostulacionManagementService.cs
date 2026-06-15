using services.dtos;

namespace services.interfaces;

public interface IPostulacionManagementService
{
    Task<IReadOnlyCollection<VacanteConPostulantesResponse>> GetPostulantesAgrupadosByVacanteAsync(
        Guid employerUserId,
        CancellationToken cancellationToken);

    Task<UpdatePostulacionStatusResponse> UpdatePostulacionStatusAsync(
        Guid employerUserId,
        Guid postulacionId,
        UpdatePostulacionStatusRequest request,
        CancellationToken cancellationToken);
}

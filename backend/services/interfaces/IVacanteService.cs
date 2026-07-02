using services.dtos;

namespace services.interfaces;

public interface IVacanteService
{
    Task<IReadOnlyCollection<VacanteResponse>> GetActiveVacantesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VacanteResponse>> GetAllVacantesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VacanteResponse>> GetMyVacantesAsync(Guid employerUserId, CancellationToken cancellationToken);
    Task<VacanteResponse> CreateVacanteAsync(Guid employerUserId, CreateVacanteRequest request, CancellationToken cancellationToken);
    Task<VacanteResponse> UpdateVacanteStatusAsync(Guid employerUserId, Guid vacanteId, bool isActive, CancellationToken cancellationToken);
    Task<VacanteResponse> UpdateVacanteAsync(Guid employerUserId, Guid vacanteId, UpdateVacanteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmployerPostulacionResponse>> GetPostulacionesByVacanteAsync(Guid employerUserId, Guid vacanteId, CancellationToken cancellationToken);
    Task<EmployerPostulacionResponse> RequestInterviewAsync(Guid employerUserId, Guid postulacionId, CancellationToken cancellationToken);
    Task<EmployerPostulacionResponse> DeclinePostulacionAsync(Guid employerUserId, Guid postulacionId, CancellationToken cancellationToken);
    Task ApplyAsync(Guid candidateUserId, ApplyToVacanteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PostulacionResponse>> GetMyPostulacionesAsync(Guid candidateUserId, CancellationToken cancellationToken);
    Task DeleteMyPostulacionAsync(Guid candidateUserId, Guid postulacionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotificacionResponse>> GetMyNotificacionesAsync(Guid candidateUserId, CancellationToken cancellationToken);
    Task<int> GetMyUnreadNotificacionCountAsync(Guid candidateUserId, CancellationToken cancellationToken);
    Task MarkMyPostulacionNotificationsReadAsync(Guid candidateUserId, CancellationToken cancellationToken);
    Task<VacanteResponse> UpdateMyVacanteStatusAsync(
        Guid employerUserId,
        Guid vacanteId,
        UpdateVacanteStatusRequest request,
        CancellationToken cancellationToken);
    Task<VacanteResponse> UpdateVacanteStatusAsAdminAsync(
        Guid vacanteId,
        UpdateVacanteStatusRequest request,
        CancellationToken cancellationToken);
}

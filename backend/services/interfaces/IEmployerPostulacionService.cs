using services.dtos;

namespace services.interfaces;

public interface IEmployerPostulacionService
{
    Task<IReadOnlyCollection<PostulacionSummaryResponse>> GetPostulacionesByVacanteAsync(
        Guid employerUserId,
        Guid vacanteId,
        CancellationToken cancellationToken);

    Task<PostulacionDetailResponse> GetPostulacionDetailAsync(
        Guid employerUserId,
        Guid postulacionId,
        CancellationToken cancellationToken);

    Task UpdatePostulacionStatusAsync(
        Guid employerUserId,
        Guid postulacionId,
        string newStatus,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotificacionResponse>> GetNotificacionesAsync(
        Guid employerUserId,
        Guid? vacanteId,
        CancellationToken cancellationToken);

    Task MarkNotificacionReadAsync(
        Guid employerUserId,
        Guid notificacionId,
        CancellationToken cancellationToken);

    Task<int> GetUnreadNotificacionCountAsync(
        Guid employerUserId,
        CancellationToken cancellationToken);
}

using domain.constants;
using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class EmployerPostulacionService(
    IEmployerRepository employerRepository,
    IVacanteRepository vacanteRepository,
    IPostulacionRepository postulacionRepository,
    INotificacionRepository notificacionRepository,
    IContactoAccesoRepository contactoAccesoRepository) : IEmployerPostulacionService
{
    public async Task<IReadOnlyCollection<PostulacionSummaryResponse>> GetPostulacionesByVacanteAsync(
        Guid employerUserId,
        Guid vacanteId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await ResolveEmployerAsync(employerUserId, cancellationToken);
        await VerifyVacanteOwnershipAsync(vacanteId, employer.Id, cancellationToken);

        IReadOnlyCollection<Postulacion> postulaciones =
            await postulacionRepository.GetByVacanteForEmployerAsync(
                vacanteId,
                employer.Id,
                cancellationToken);

        return postulaciones
            .Select(p => new PostulacionSummaryResponse(
                p.Id,
                p.CandidateProfileId,
                p.CandidateFullName,
                p.Status,
                p.AppliedAt,
                p.UpdatedAtUtc))
            .ToArray();
    }

    public async Task<PostulacionDetailResponse> GetPostulacionDetailAsync(
        Guid employerUserId,
        Guid postulacionId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await ResolveEmployerAsync(employerUserId, cancellationToken);

        Postulacion postulacion = await postulacionRepository.FindByIdForEmployerAsync(
                postulacionId,
                employer.Id,
                cancellationToken)
            ?? throw new NotFoundException("La postulación no existe.");

        DateTime now = DateTime.UtcNow;
        string effectiveStatus = postulacion.Status;
        DateTime effectiveUpdatedAt = postulacion.UpdatedAtUtc;

        if (postulacion.Status == PostulacionStatuses.Enviada)
        {
            await postulacionRepository.UpdateStatusForEmployerAsync(
                postulacionId,
                employer.Id,
                PostulacionStatuses.Vista,
                now,
                cancellationToken);

            effectiveStatus = PostulacionStatuses.Vista;
            effectiveUpdatedAt = now;
        }

        await contactoAccesoRepository.SaveAsync(
            new ContactoAcceso
            {
                Id = Guid.NewGuid(),
                EmployerProfileId = employer.Id,
                PostulacionId = postulacionId,
                CandidateEmail = postulacion.CandidateEmail,
                AccessedAtUtc = DateTime.UtcNow,
            },
            cancellationToken);

        int age = CalculateAge(postulacion.CandidateDateOfBirth);

        return new PostulacionDetailResponse(
            postulacion.Id,
            postulacion.VacanteId,
            postulacion.JobTitle,
            effectiveStatus,
            postulacion.AppliedAt,
            effectiveUpdatedAt,
            postulacion.CandidateProfileId,
            postulacion.CandidateFullName,
            postulacion.CandidateEmail,
            postulacion.CandidateProvince,
            postulacion.CandidateEducationLevel,
            postulacion.CandidateDateOfBirth,
            age);
    }

    private static readonly IReadOnlySet<string> EmployerManagedStatuses = new HashSet<string>
    {
        PostulacionStatuses.EnRevision,
        PostulacionStatuses.EntrevistaSolicitada,
        PostulacionStatuses.Entrevista,
        PostulacionStatuses.Finalizada,
    };

    public async Task UpdatePostulacionStatusAsync(
        Guid employerUserId,
        Guid postulacionId,
        string newStatus,
        CancellationToken cancellationToken)
    {
        if (!EmployerManagedStatuses.Contains(newStatus))
        {
            throw new RequestValidationException(
                [$"Estado inválido. Los valores permitidos son: {string.Join(", ", EmployerManagedStatuses)}"]);
        }

        EmployerProfile employer = await ResolveEmployerAsync(employerUserId, cancellationToken);

        Postulacion postulacion = await postulacionRepository.FindByIdForEmployerAsync(
                postulacionId,
                employer.Id,
                cancellationToken)
            ?? throw new NotFoundException("La postulación no existe.");

        if (postulacion.Status == newStatus)
        {
            throw new RequestValidationException([$"La postulación ya se encuentra en estado '{newStatus}'."]);
        }

        await postulacionRepository.UpdateStatusForEmployerAsync(
            postulacionId,
            employer.Id,
            newStatus,
            DateTime.UtcNow,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificacionResponse>> GetNotificacionesAsync(
        Guid employerUserId,
        Guid? vacanteId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await ResolveEmployerAsync(employerUserId, cancellationToken);

        IReadOnlyCollection<Notificacion> notificaciones =
            await notificacionRepository.GetByEmployerProfileIdAsync(employer.Id, vacanteId, cancellationToken);

        await notificacionRepository.MarkEmployerVacanteAsReadAsync(employer.Id, vacanteId, cancellationToken);

        return notificaciones
            .Select(n => new NotificacionResponse(
                n.Id,
                n.PostulacionId,
                n.VacanteId,
                n.JobTitle,
                n.Message,
                n.IsRead,
                n.CreatedAtUtc))
            .ToArray();
    }

    public async Task MarkNotificacionReadAsync(
        Guid employerUserId,
        Guid notificacionId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await ResolveEmployerAsync(employerUserId, cancellationToken);

        await notificacionRepository.MarkAsReadAsync(notificacionId, employer.Id, cancellationToken);
    }

    public async Task<int> GetUnreadNotificacionCountAsync(
        Guid employerUserId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await ResolveEmployerAsync(employerUserId, cancellationToken);

        return await notificacionRepository.GetUnreadCountAsync(employer.Id, cancellationToken);
    }

    private async Task<EmployerProfile> ResolveEmployerAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await employerRepository.FindByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("No se encontró el perfil del empleador.");
    }

    private async Task VerifyVacanteOwnershipAsync(Guid vacanteId, Guid employerProfileId, CancellationToken cancellationToken)
    {
        Vacante? vacante = await vacanteRepository.FindByIdAsync(vacanteId, cancellationToken);

        if (vacante is null || vacante.EmployerProfileId != employerProfileId)
        {
            throw new NotFoundException("La vacante no existe.");
        }
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int age = today.Year - dateOfBirth.Year;

        if (today < dateOfBirth.AddYears(age))
            age--;

        return age;
    }
}

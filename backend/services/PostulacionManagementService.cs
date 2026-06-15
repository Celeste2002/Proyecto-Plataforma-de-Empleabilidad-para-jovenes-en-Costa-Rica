using domain.constants;
using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class PostulacionManagementService(
    IVacanteRepository vacanteRepository,
    IPostulacionRepository postulacionRepository,
    IEmployerRepository employerRepository) : IPostulacionManagementService
{
    public async Task<IReadOnlyCollection<VacanteConPostulantesResponse>> GetPostulantesAgrupadosByVacanteAsync(
        Guid employerUserId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await RequireEmployerAsync(employerUserId, cancellationToken);

        IReadOnlyCollection<Vacante> vacantes =
            await vacanteRepository.GetByEmployerProfileIdAsync(employer.Id, cancellationToken);

        List<VacanteConPostulantesResponse> result = [];

        foreach (Vacante vacante in vacantes)
        {
            IReadOnlyCollection<Postulacion> postulaciones =
                await postulacionRepository.GetByVacanteIdWithCandidateAsync(vacante.Id, cancellationToken);

            List<PostulacionConCandidatoResponse> postulantes = postulaciones
                .Select(p => new PostulacionConCandidatoResponse(
                    PostulacionId: p.Id,
                    Status: p.Status,
                    AppliedAt: p.AppliedAt,
                    UpdatedAtUtc: p.UpdatedAtUtc,
                    CandidateProfileId: p.CandidateProfileId,
                    CandidateFullName: p.CandidateFullName,
                    CandidateEmail: p.CandidateEmail,
                    CandidateProvince: p.CandidateProvince,
                    CandidateEducationLevel: p.CandidateEducationLevel,
                    CandidateAge: p.CandidateAge))
                .ToList();

            result.Add(new VacanteConPostulantesResponse(
                VacanteId: vacante.Id,
                JobTitle: vacante.JobTitle,
                Province: vacante.Province,
                Sector: vacante.Sector,
                Modality: vacante.Modality,
                IsActive: vacante.IsActive,
                PublishedAt: vacante.PublishedAt,
                Postulantes: postulantes));
        }

        return result;
    }

    public async Task<UpdatePostulacionStatusResponse> UpdatePostulacionStatusAsync(
        Guid employerUserId,
        Guid postulacionId,
        UpdatePostulacionStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!PostulacionStatuses.EmployerSettable.Contains(request.NewStatus))
        {
            throw new RequestValidationException(
            [
                $"El estado '{request.NewStatus}' no es válido. Estados permitidos: {string.Join(", ", PostulacionStatuses.EmployerSettable)}."
            ]);
        }

        EmployerProfile employer = await RequireEmployerAsync(employerUserId, cancellationToken);

        Postulacion? postulacion =
            await postulacionRepository.FindByIdAsync(postulacionId, cancellationToken);

        if (postulacion is null)
            throw new NotFoundException("La postulación no existe.");

        Vacante? vacante =
            await vacanteRepository.FindByIdAsync(postulacion.VacanteId, cancellationToken);

        if (vacante is null || vacante.EmployerProfileId != employer.Id)
            throw new NotFoundException("La postulación no pertenece a ninguna vacante de este empleador.");

        string oldStatus = postulacion.Status;
        DateTime updatedAt = DateTime.UtcNow;

        await postulacionRepository.UpdateStatusAsync(postulacionId, request.NewStatus, updatedAt, cancellationToken);

        return new UpdatePostulacionStatusResponse(
            PostulacionId: postulacionId,
            OldStatus: oldStatus,
            NewStatus: request.NewStatus,
            UpdatedAtUtc: updatedAt);
    }

    private async Task<EmployerProfile> RequireEmployerAsync(Guid userId, CancellationToken cancellationToken)
    {
        EmployerProfile? employer = await employerRepository.FindByUserIdAsync(userId, cancellationToken);

        if (employer is null)
            throw new NotFoundException("No se encontró el perfil del empleador.");

        return employer;
    }
}

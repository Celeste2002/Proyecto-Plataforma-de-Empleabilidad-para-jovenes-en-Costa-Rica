using domain.constants;
using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class VacanteService(
    IVacanteRepository vacanteRepository,
    IPostulacionRepository postulacionRepository,
    ICandidateRepository candidateRepository,
    IEmployerRepository employerRepository) : IVacanteService
{
    public async Task<IReadOnlyCollection<VacanteResponse>> GetActiveVacantesAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Vacante> vacantes =
            await vacanteRepository.GetActiveAsync(cancellationToken);

        return vacantes
            .Select(MapVacanteResponse)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<VacanteResponse>> GetAllVacantesAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Vacante> vacantes =
            await vacanteRepository.GetAllAsync(cancellationToken);

        return vacantes
            .Select(MapVacanteResponse)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<VacanteResponse>> GetMyVacantesAsync(
        Guid employerUserId,
        CancellationToken cancellationToken)
    {
        EmployerProfile? employerProfile =
            await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employerProfile is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        IReadOnlyCollection<Vacante> vacantes =
            await vacanteRepository.GetByEmployerProfileIdAsync(employerProfile.Id, cancellationToken);

        return vacantes
            .Select(MapVacanteResponse)
            .ToArray();
    }

    public async Task ApplyAsync(
        Guid candidateUserId,
        ApplyToVacanteRequest request,
        CancellationToken cancellationToken)
    {
        CandidateProfile? candidateProfile =
            await candidateRepository.FindByUserIdAsync(candidateUserId, cancellationToken);

        if (candidateProfile is null)
        {
            throw new NotFoundException("No se encontró el perfil del candidato.");
        }

        Vacante? vacante = await vacanteRepository.FindByIdAsync(request.VacanteId, cancellationToken);

        if (vacante is null || !vacante.IsActive)
        {
            throw new NotFoundException("La vacante no existe o ya no está disponible.");
        }

        bool alreadyApplied = await postulacionRepository.ExistsByVacanteAndCandidateAsync(
            request.VacanteId,
            candidateProfile.Id,
            cancellationToken);

        if (alreadyApplied)
        {
            throw new RequestValidationException(["Ya te postulaste a esta vacante."]);
        }

        Postulacion postulacion = new()
        {
            Id = Guid.NewGuid(),
            VacanteId = request.VacanteId,
            CandidateProfileId = candidateProfile.Id,
            Status = PostulacionStatuses.Enviada,
            AppliedAt = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await postulacionRepository.SaveAsync(postulacion, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PostulacionResponse>> GetMyPostulacionesAsync(
        Guid candidateUserId,
        CancellationToken cancellationToken)
    {
        CandidateProfile? candidateProfile =
            await candidateRepository.FindByUserIdAsync(candidateUserId, cancellationToken);

        if (candidateProfile is null)
        {
            throw new NotFoundException("No se encontró el perfil del candidato.");
        }

        IReadOnlyCollection<Postulacion> postulaciones =
            await postulacionRepository.GetByCandidateProfileIdAsync(
                candidateProfile.Id,
                cancellationToken);

        return postulaciones
            .Select(MapPostulacionResponse)
            .ToArray();
    }

    public async Task<VacanteResponse> UpdateMyVacanteStatusAsync(
        Guid employerUserId,
        Guid vacanteId,
        UpdateVacanteStatusRequest request,
        CancellationToken cancellationToken)
    {
        EmployerProfile? employerProfile =
            await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employerProfile is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        Vacante? vacante = await vacanteRepository.FindByIdAsync(vacanteId, cancellationToken);

        if (vacante is null || vacante.EmployerProfileId != employerProfile.Id)
        {
            throw new NotFoundException("No se encontró la vacante solicitada.");
        }

        await vacanteRepository.UpdateIsActiveAsync(vacanteId, request.IsActive, cancellationToken);

        Vacante? updatedVacante = await vacanteRepository.FindByIdAsync(vacanteId, cancellationToken);

        return MapVacanteResponse(updatedVacante ?? vacante);
    }

    public async Task<VacanteResponse> UpdateVacanteStatusAsAdminAsync(
        Guid vacanteId,
        UpdateVacanteStatusRequest request,
        CancellationToken cancellationToken)
    {
        Vacante? vacante = await vacanteRepository.FindByIdAsync(vacanteId, cancellationToken);

        if (vacante is null)
        {
            throw new NotFoundException("No se encontró la vacante solicitada.");
        }

        await vacanteRepository.UpdateIsActiveAsync(vacanteId, request.IsActive, cancellationToken);

        Vacante? updatedVacante = await vacanteRepository.FindByIdAsync(vacanteId, cancellationToken);

        return MapVacanteResponse(updatedVacante ?? vacante);
    }

    private static VacanteResponse MapVacanteResponse(Vacante v) =>
        new(v.Id, v.JobTitle, v.CompanyName, v.Province, v.Sector, v.Modality, v.ExperienceLevel, v.Description, v.IsActive, v.PublishedAt);

    private static PostulacionResponse MapPostulacionResponse(Postulacion p) =>
        new(p.Id, p.VacanteId, p.JobTitle, p.CompanyName, p.Province, p.Status, p.AppliedAt, p.UpdatedAtUtc);
}

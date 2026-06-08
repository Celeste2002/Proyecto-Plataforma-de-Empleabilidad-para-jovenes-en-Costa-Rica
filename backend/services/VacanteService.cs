using domain.constants;
using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class VacanteService(
    IVacanteRepository vacanteRepository,
    IPostulacionRepository postulacionRepository,
    ICandidateRepository candidateRepository) : IVacanteService
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

    private static VacanteResponse MapVacanteResponse(Vacante v) =>
        new(v.Id, v.JobTitle, v.CompanyName, v.Province, v.Sector, v.Modality, v.ExperienceLevel, v.Description, v.PublishedAt);

    private static PostulacionResponse MapPostulacionResponse(Postulacion p) =>
        new(p.Id, p.VacanteId, p.JobTitle, p.CompanyName, p.Province, p.Status, p.AppliedAt, p.UpdatedAtUtc);
}

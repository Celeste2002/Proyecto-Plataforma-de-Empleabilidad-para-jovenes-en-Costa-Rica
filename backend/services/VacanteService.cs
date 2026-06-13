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

    public async Task<IReadOnlyCollection<VacanteResponse>> GetMyVacantesAsync(
        Guid employerUserId,
        CancellationToken cancellationToken)
    {
        EmployerProfile? employer =
            await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employer is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        IReadOnlyCollection<Vacante> vacantes =
            await vacanteRepository.GetByEmployerProfileIdAsync(employer.Id, cancellationToken);

        return vacantes
            .Select(MapVacanteResponse)
            .ToArray();
    }

    public async Task<VacanteResponse> CreateVacanteAsync(
        Guid employerUserId,
        CreateVacanteRequest request,
        CancellationToken cancellationToken)
    {
        EmployerProfile? employer =
            await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employer is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(request.JobTitle) || request.JobTitle.Length > 100)
            errors.Add("El título del puesto es obligatorio y no puede superar los 100 caracteres.");

        if (!CandidateCatalogs.CostaRicaProvinces.Contains(request.Province))
            errors.Add("La provincia seleccionada no es válida.");

        if (!EmployerSectors.All.Contains(request.Sector))
            errors.Add("El sector seleccionado no es válido.");

        if (!VacanteModalities.All.Contains(request.Modality))
            errors.Add("La modalidad seleccionada no es válida.");

        if (!ExperienceLevels.All.Contains(request.ExperienceLevel))
            errors.Add("El nivel de experiencia seleccionado no es válido.");

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        DateTime now = DateTime.UtcNow;

        Vacante vacante = new()
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = employer.Id,
            JobTitle = request.JobTitle.Trim(),
            Province = request.Province,
            Sector = request.Sector,
            Modality = request.Modality,
            ExperienceLevel = request.ExperienceLevel,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim(),
            SalaryRange = string.IsNullOrWhiteSpace(request.SalaryRange) ? null : request.SalaryRange.Trim(),
            IsActive = true,
            PublishedAt = now,
            CreatedAtUtc = now,
            CompanyName = employer.CompanyName
        };

        await vacanteRepository.SaveAsync(vacante, cancellationToken);

        return MapVacanteResponse(vacante);
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
        new(v.Id, v.JobTitle, v.CompanyName, v.Province, v.Sector, v.Modality, v.ExperienceLevel,
            v.Description, v.Requirements, v.SalaryRange, v.PublishedAt);

    private static PostulacionResponse MapPostulacionResponse(Postulacion p) =>
        new(p.Id, p.VacanteId, p.JobTitle, p.CompanyName, p.Province, p.Status, p.AppliedAt, p.UpdatedAtUtc);
}

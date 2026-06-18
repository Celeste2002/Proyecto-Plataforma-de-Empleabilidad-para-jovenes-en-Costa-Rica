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
    IEmployerRepository employerRepository,
    INotificacionRepository notificacionRepository,
    IInterviewRequestSender interviewRequestSender) : IVacanteService
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

        string? description = NormalizeOptionalText(request.Description);
        string? requirements = NormalizeOptionalText(request.Requirements);
        string? salaryRange = NormalizeOptionalText(request.SalaryRange);

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

        if (salaryRange is { Length: > 100 })
            errors.Add("El rango salarial no puede superar los 100 caracteres.");

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
            Description = description,
            Requirements = requirements,
            SalaryRange = salaryRange,
            IsActive = true,
            PublishedAt = now,
            CreatedAtUtc = now,
            CompanyName = employer.CompanyName
        };

        await vacanteRepository.SaveAsync(vacante, cancellationToken);

        return MapVacanteResponse(vacante);
    }

    public async Task<VacanteResponse> UpdateVacanteAsync(
        Guid employerUserId,
        Guid vacanteId,
        UpdateVacanteRequest request,
        CancellationToken cancellationToken)
    {
        EmployerProfile? employer =
            await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employer is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        Vacante? currentVacante = await vacanteRepository.FindByIdAsync(vacanteId, cancellationToken);

        if (currentVacante is null || currentVacante.EmployerProfileId != employer.Id)
        {
            throw new NotFoundException("No se encontró la vacante solicitada.");
        }

        if (!currentVacante.IsActive)
        {
            throw new RequestValidationException(["Solo se pueden editar vacantes activas."]);
        }

        string? description = NormalizeOptionalText(request.Description);
        string? requirements = NormalizeOptionalText(request.Requirements);
        string? salaryRange = NormalizeOptionalText(request.SalaryRange);

        List<string> errors = [];

        if (salaryRange is { Length: > 100 })
        {
            errors.Add("El rango salarial no puede superar los 100 caracteres.");
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        bool updated = await vacanteRepository.UpdateEditableFieldsAsync(
            currentVacante.Id,
            employer.Id,
            description,
            requirements,
            salaryRange,
            cancellationToken);

        if (!updated)
        {
            throw new RequestValidationException(["La vacante ya no está activa o no se pudo actualizar."]);
        }

        Vacante updatedVacante = new()
        {
            Id = currentVacante.Id,
            EmployerProfileId = currentVacante.EmployerProfileId,
            JobTitle = currentVacante.JobTitle,
            Province = currentVacante.Province,
            Sector = currentVacante.Sector,
            Modality = currentVacante.Modality,
            ExperienceLevel = currentVacante.ExperienceLevel,
            Description = description,
            Requirements = requirements,
            SalaryRange = salaryRange,
            IsActive = currentVacante.IsActive,
            PublishedAt = currentVacante.PublishedAt,
            CreatedAtUtc = currentVacante.CreatedAtUtc,
            CompanyName = currentVacante.CompanyName
        };

        return MapVacanteResponse(updatedVacante);
    }

    public async Task<IReadOnlyCollection<EmployerPostulacionResponse>> GetPostulacionesByVacanteAsync(
        Guid employerUserId,
        Guid vacanteId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await GetEmployerProfileAsync(employerUserId, cancellationToken);
        Vacante vacante = await GetOwnedVacanteAsync(employer.Id, vacanteId, cancellationToken);

        IReadOnlyCollection<Postulacion> postulaciones =
            await postulacionRepository.GetByVacanteForEmployerAsync(
                vacante.Id,
                employer.Id,
                cancellationToken);

        return postulaciones
            .Select(MapEmployerPostulacionResponse)
            .ToArray();
    }

    public async Task<EmployerPostulacionResponse> RequestInterviewAsync(
        Guid employerUserId,
        Guid postulacionId,
        CancellationToken cancellationToken)
    {
        EmployerProfile employer = await GetEmployerProfileAsync(employerUserId, cancellationToken);

        Postulacion? postulacion =
            await postulacionRepository.FindByIdForEmployerAsync(
                postulacionId,
                employer.Id,
                cancellationToken);

        if (postulacion is null)
        {
            throw new NotFoundException("No se encontró la postulación solicitada.");
        }

        if (postulacion.Status == PostulacionStatuses.EntrevistaSolicitada)
        {
            throw new RequestValidationException(["La entrevista ya fue solicitada para esta postulación."]);
        }

        if (postulacion.Status == PostulacionStatuses.Finalizada)
        {
            throw new RequestValidationException(["No se puede solicitar entrevista en una postulación finalizada."]);
        }

        await interviewRequestSender.SendInterviewRequestAsync(
            employer,
            postulacion,
            cancellationToken);

        DateTime updatedAtUtc = DateTime.UtcNow;

        bool updated = await postulacionRepository.UpdateStatusForEmployerAsync(
            postulacion.Id,
            employer.Id,
            PostulacionStatuses.EntrevistaSolicitada,
            updatedAtUtc,
            cancellationToken);

        if (!updated)
        {
            throw new RequestValidationException(["No se pudo actualizar el estado de la postulación."]);
        }

        Postulacion updatedPostulacion = new()
        {
            Id = postulacion.Id,
            VacanteId = postulacion.VacanteId,
            CandidateProfileId = postulacion.CandidateProfileId,
            Status = PostulacionStatuses.EntrevistaSolicitada,
            AppliedAt = postulacion.AppliedAt,
            UpdatedAtUtc = updatedAtUtc,
            JobTitle = postulacion.JobTitle,
            CompanyName = postulacion.CompanyName,
            Province = postulacion.Province,
            CandidateFullName = postulacion.CandidateFullName,
            CandidateEmail = postulacion.CandidateEmail,
            CandidateProvince = postulacion.CandidateProvince,
            CandidateEducationLevel = postulacion.CandidateEducationLevel
        };

        return MapEmployerPostulacionResponse(updatedPostulacion);
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

        Notificacion notificacion = new()
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = vacante.EmployerProfileId,
            PostulacionId = postulacion.Id,
            VacanteId = vacante.Id,
            Message = $"{candidateProfile.FullName} se ha postulado a la vacante {vacante.JobTitle}",
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await notificacionRepository.SaveAsync(notificacion, cancellationToken);
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
            v.Description, v.Requirements, v.SalaryRange, v.IsActive, v.PublishedAt);

    private static PostulacionResponse MapPostulacionResponse(Postulacion p) =>
        new(p.Id, p.VacanteId, p.JobTitle, p.CompanyName, p.Province, p.Status, p.AppliedAt, p.UpdatedAtUtc);

    private static EmployerPostulacionResponse MapEmployerPostulacionResponse(Postulacion p) =>
        new(
            p.Id,
            p.VacanteId,
            p.CandidateProfileId,
            p.CandidateFullName,
            p.CandidateEmail,
            p.CandidateProvince,
            p.CandidateEducationLevel,
            p.Status,
            p.AppliedAt,
            p.UpdatedAtUtc);

    private async Task<EmployerProfile> GetEmployerProfileAsync(
        Guid employerUserId,
        CancellationToken cancellationToken)
    {
        EmployerProfile? employer =
            await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employer is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        return employer;
    }

    private async Task<Vacante> GetOwnedVacanteAsync(
        Guid employerProfileId,
        Guid vacanteId,
        CancellationToken cancellationToken)
    {
        Vacante? vacante = await vacanteRepository.FindByIdAsync(vacanteId, cancellationToken);

        if (vacante is null || vacante.EmployerProfileId != employerProfileId)
        {
            throw new NotFoundException("No se encontró la vacante solicitada.");
        }

        return vacante;
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

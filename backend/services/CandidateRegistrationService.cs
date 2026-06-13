using System.Net.Mail;
using domain.constants;
using domain.entities;
using Microsoft.Extensions.Logging;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class CandidateRegistrationService(
    ICandidateRepository candidateRepository,
    IUserRepository userRepository,
    IEmailConfirmationSender emailConfirmationSender,
    IPasswordHasher passwordHasher,
    ILogger<CandidateRegistrationService> logger) : ICandidateRegistrationService
{
    public async Task<CandidateRegistrationResponse> RegisterAsync(
        RegisterCandidateRequest registerCandidateRequest,
        CancellationToken cancellationToken)
    {
        ValidateRegisterCandidateRequest(registerCandidateRequest);

        string normalizedEmail = registerCandidateRequest.Email.Trim().ToLowerInvariant();

        User? existingUser = await userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
        string passwordHash = passwordHasher.Hash(registerCandidateRequest.Password);
        DateTime createdAt = DateTime.UtcNow;
        User registrationUser;

        if (existingUser is not null)
        {
            CandidateProfile? existingCandidateProfile =
                await candidateRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

            if (existingCandidateProfile is not null || existingUser.Role != UserRoles.Candidate)
            {
                throw new RequestValidationException(["Ya existe un candidato registrado con este correo."]);
            }

            if (!existingUser.IsActive)
            {
                throw new RequestValidationException(["Este correo esta asociado a una cuenta inactiva."]);
            }

            await userRepository.UpdatePasswordAsync(existingUser.Id, passwordHash, cancellationToken);
            registrationUser = existingUser with
            {
                PasswordHash = passwordHash
            };
        }
        else
        {
            registrationUser = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = UserRoles.Candidate,
                IsActive = true,
                EmailConfirmed = false,
                CreatedAtUtc = createdAt
            };

            await userRepository.SaveAsync(registrationUser, cancellationToken);
        }

        CandidateProfile candidateProfile = new()
        {
            Id = Guid.NewGuid(),
            UserId = registrationUser.Id,
            FullName = registerCandidateRequest.FullName.Trim(),
            DateOfBirth = registerCandidateRequest.DateOfBirth,
            Province = registerCandidateRequest.Province.Trim(),
            EducationLevel = registerCandidateRequest.EducationLevel.Trim(),
            Email = normalizedEmail,
            CreatedAtUtc = createdAt,
            IsVisibleToPartnerEmployers = true,
            EmailConfirmationSent = false
        };

        await candidateRepository.SaveAsync(candidateProfile, cancellationToken);

        bool emailSent = false;

        try
        {
            await emailConfirmationSender.SendRegistrationConfirmationAsync(candidateProfile, cancellationToken);
            await candidateRepository.MarkEmailConfirmationSentAsync(candidateProfile.Id, cancellationToken);
            emailSent = true;
        }
        catch (EmailDeliveryException emailDeliveryException)
        {
            logger.LogWarning(emailDeliveryException,
                "No se pudo enviar el correo de confirmación al candidato {CandidateId}. El perfil fue guardado.",
                candidateProfile.Id);
        }

        CandidateProfile savedCandidateProfile = candidateProfile with
        {
            EmailConfirmationSent = emailSent
        };

        string message = emailSent
            ? "Registro completado correctamente. Te enviamos un correo de confirmación."
            : "Registro completado correctamente. No se pudo enviar el correo de confirmación, pero tu perfil fue guardado.";

        return new CandidateRegistrationResponse(
            MapCandidateProfileResponse(savedCandidateProfile),
            message);
    }

    public async Task<IReadOnlyCollection<CandidateProfileResponse>> GetProfilesVisibleToPartnerEmployersAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<CandidateProfile> candidateProfiles =
            await candidateRepository.GetVisibleToPartnerEmployersAsync(cancellationToken);

        return candidateProfiles
            .OrderByDescending(candidateProfile => candidateProfile.CreatedAtUtc)
            .Select(MapCandidateProfileResponse)
            .ToArray();
    }

    public async Task<CandidateProfileResponse> GetProfileByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        CandidateProfile? candidateProfile =
            await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (candidateProfile is null)
        {
            throw new NotFoundException("No se encontro el perfil del candidato.");
        }

        return MapCandidateProfileResponse(candidateProfile);
    }

    public async Task<CandidatoPerfilCompletoResponse> GetFullProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException("No se encontro el perfil del candidato.");
        }

        IReadOnlyCollection<domain.entities.ExperienciaLaboral> experiencias =
            await candidateRepository.GetExperienciasAsync(profile.Id, cancellationToken);

        IReadOnlyCollection<domain.entities.Habilidad> habilidades =
            await candidateRepository.GetHabilidadesAsync(profile.Id, cancellationToken);

        IReadOnlyCollection<domain.entities.CursoCompletado> cursos =
            await candidateRepository.GetCursosAsync(profile.Id, cancellationToken);

        return new CandidatoPerfilCompletoResponse(
            profile.Id,
            profile.FullName,
            profile.DateOfBirth,
            CalculateAge(profile.DateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow)),
            profile.Province,
            profile.EducationLevel,
            profile.Email,
            profile.IsAvailableForContact,
            profile.IsVisibleToPartnerEmployers,
            profile.PhotoUrl,
            experiencias.Select(e => new ExperienciaLaboralResponse(
                e.Id, e.Empresa, e.Cargo, e.FechaInicio, e.FechaFin, e.EsTrabajoActual, e.Descripcion)).ToArray(),
            habilidades.Select(h => new HabilidadResponse(h.Id, h.Nombre)).ToArray(),
            cursos.Select(c => new CursoCompletadoResponse(
                c.Id, c.NombreCurso, c.Institucion, c.FechaCompletado, c.EsDePlataforma)).ToArray());
    }

    public async Task UpdateAvailabilityAsync(
        Guid userId,
        bool isAvailableForContact,
        CancellationToken cancellationToken)
    {
        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException("No se encontro el perfil del candidato.");
        }

        await candidateRepository.UpdateAvailabilityAsync(profile.Id, isAvailableForContact, cancellationToken);
    }

    public async Task<ExperienciaLaboralResponse> AddExperienciaAsync(
        Guid userId,
        AddExperienciaLaboralRequest request,
        CancellationToken cancellationToken)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(request.Empresa))
            errors.Add("El nombre de la empresa es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Cargo))
            errors.Add("El cargo es obligatorio.");
        if (!request.EsTrabajoActual && request.FechaFin.HasValue && request.FechaFin < request.FechaInicio)
            errors.Add("La fecha de fin no puede ser anterior a la fecha de inicio.");

        if (errors.Count > 0)
            throw new RequestValidationException(errors);

        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("No se encontro el perfil del candidato.");

        domain.entities.ExperienciaLaboral experiencia = new()
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            Empresa = request.Empresa.Trim(),
            Cargo = request.Cargo.Trim(),
            FechaInicio = request.FechaInicio,
            FechaFin = request.EsTrabajoActual ? null : request.FechaFin,
            EsTrabajoActual = request.EsTrabajoActual,
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim()
        };

        await candidateRepository.SaveExperienciaAsync(experiencia, cancellationToken);

        return new ExperienciaLaboralResponse(
            experiencia.Id, experiencia.Empresa, experiencia.Cargo,
            experiencia.FechaInicio, experiencia.FechaFin,
            experiencia.EsTrabajoActual, experiencia.Descripcion);
    }

    public async Task DeleteExperienciaAsync(Guid userId, Guid experienciaId, CancellationToken cancellationToken)
    {
        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("No se encontro el perfil del candidato.");

        await candidateRepository.DeleteExperienciaAsync(experienciaId, profile.Id, cancellationToken);
    }

    public async Task<HabilidadResponse> AddHabilidadAsync(
        Guid userId,
        AddHabilidadRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new RequestValidationException(["El nombre de la habilidad es obligatorio."]);

        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("No se encontro el perfil del candidato.");

        domain.entities.Habilidad habilidad = new()
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            Nombre = request.Nombre.Trim()
        };

        await candidateRepository.SaveHabilidadAsync(habilidad, cancellationToken);

        return new HabilidadResponse(habilidad.Id, habilidad.Nombre);
    }

    public async Task DeleteHabilidadAsync(Guid userId, Guid habilidadId, CancellationToken cancellationToken)
    {
        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("No se encontro el perfil del candidato.");

        await candidateRepository.DeleteHabilidadAsync(habilidadId, profile.Id, cancellationToken);
    }

    public async Task<CursoCompletadoResponse> AddCursoAsync(
        Guid userId,
        AddCursoCompletadoRequest request,
        CancellationToken cancellationToken)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(request.NombreCurso))
            errors.Add("El nombre del curso es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Institucion))
            errors.Add("La institución es obligatoria.");

        if (errors.Count > 0)
            throw new RequestValidationException(errors);

        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("No se encontro el perfil del candidato.");

        domain.entities.CursoCompletado curso = new()
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            NombreCurso = request.NombreCurso.Trim(),
            Institucion = request.Institucion.Trim(),
            FechaCompletado = request.FechaCompletado,
            EsDePlataforma = request.EsDePlataforma
        };

        await candidateRepository.SaveCursoAsync(curso, cancellationToken);

        return new CursoCompletadoResponse(
            curso.Id, curso.NombreCurso, curso.Institucion, curso.FechaCompletado, curso.EsDePlataforma);
    }

    public async Task DeleteCursoAsync(Guid userId, Guid cursoId, CancellationToken cancellationToken)
    {
        CandidateProfile? profile = await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("No se encontro el perfil del candidato.");

        await candidateRepository.DeleteCursoAsync(cursoId, profile.Id, cancellationToken);
    }

    public async Task<CandidateProfileResponse> UpdateProfileAsync(
        Guid userId,
        UpdateCandidateProfileRequest updateCandidateProfileRequest,
        CancellationToken cancellationToken)
    {
        ValidateCandidateProfileData(
            updateCandidateProfileRequest.FullName,
            updateCandidateProfileRequest.DateOfBirth,
            updateCandidateProfileRequest.Province,
            updateCandidateProfileRequest.EducationLevel);

        CandidateProfile? existingProfile =
            await candidateRepository.FindByUserIdAsync(userId, cancellationToken);

        if (existingProfile is null)
        {
            throw new NotFoundException("No se encontro el perfil del candidato.");
        }

        CandidateProfile updatedProfile = existingProfile with
        {
            FullName = updateCandidateProfileRequest.FullName.Trim(),
            DateOfBirth = updateCandidateProfileRequest.DateOfBirth,
            Province = updateCandidateProfileRequest.Province.Trim(),
            EducationLevel = updateCandidateProfileRequest.EducationLevel.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(updateCandidateProfileRequest.PhotoUrl)
                ? null
                : updateCandidateProfileRequest.PhotoUrl.Trim()
        };

        await candidateRepository.UpdateAsync(updatedProfile, cancellationToken);

        return MapCandidateProfileResponse(updatedProfile);
    }

    public async Task UpdatePasswordAsync(
        Guid userId,
        UpdateCandidatePasswordRequest updateCandidatePasswordRequest,
        CancellationToken cancellationToken)
    {
        List<string> validationErrors = [];

        if (string.IsNullOrWhiteSpace(updateCandidatePasswordRequest.CurrentPassword))
        {
            validationErrors.Add("La contraseña actual es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(updateCandidatePasswordRequest.NewPassword) ||
            updateCandidatePasswordRequest.NewPassword.Length < 8)
        {
            validationErrors.Add("La nueva contraseña debe tener al menos 8 caracteres.");
        }

        if (validationErrors.Count > 0)
        {
            throw new RequestValidationException(validationErrors);
        }

        User? user = await userRepository.FindByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive || user.Role != UserRoles.Candidate)
        {
            throw new NotFoundException("No se encontro el usuario candidato.");
        }

        if (user.PasswordHash is null ||
            !passwordHasher.Verify(updateCandidatePasswordRequest.CurrentPassword, user.PasswordHash))
        {
            throw new RequestValidationException(["La contraseña actual no es correcta."]);
        }

        string newPasswordHash = passwordHasher.Hash(updateCandidatePasswordRequest.NewPassword);

        await userRepository.UpdatePasswordAsync(user.Id, newPasswordHash, cancellationToken);
    }

    private static void ValidateRegisterCandidateRequest(RegisterCandidateRequest registerCandidateRequest)
    {
        List<string> validationErrors = [];

        ValidateCandidateProfileData(
            registerCandidateRequest.FullName,
            registerCandidateRequest.DateOfBirth,
            registerCandidateRequest.Province,
            registerCandidateRequest.EducationLevel,
            validationErrors);

        if (!IsValidEmail(registerCandidateRequest.Email))
        {
            validationErrors.Add("El correo electrónico no tiene un formato válido.");
        }

        if (string.IsNullOrWhiteSpace(registerCandidateRequest.Password) ||
            registerCandidateRequest.Password.Length < 8)
        {
            validationErrors.Add("La contraseña debe tener al menos 8 caracteres.");
        }

        if (validationErrors.Count > 0)
        {
            throw new RequestValidationException(validationErrors);
        }
    }

    private static void ValidateCandidateProfileData(
        string fullName,
        DateOnly dateOfBirth,
        string province,
        string educationLevel)
    {
        List<string> validationErrors = [];

        ValidateCandidateProfileData(fullName, dateOfBirth, province, educationLevel, validationErrors);

        if (validationErrors.Count > 0)
        {
            throw new RequestValidationException(validationErrors);
        }
    }

    private static void ValidateCandidateProfileData(
        string fullName,
        DateOnly dateOfBirth,
        string province,
        string educationLevel,
        List<string> validationErrors)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            validationErrors.Add("El nombre es obligatorio.");
        }

        int age = CalculateAge(dateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow));

        if (age is < 18 or > 30)
        {
            validationErrors.Add("La fecha de nacimiento debe corresponder a una edad entre 18 y 30 años.");
        }

        if (!CandidateCatalogs.CostaRicaProvinces.Contains(province.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            validationErrors.Add("La provincia debe ser una provincia valida de Costa Rica.");
        }

        if (!CandidateCatalogs.EducationLevels.Contains(educationLevel.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            validationErrors.Add("El nivel educativo no es valido.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            _ = new MailAddress(email.Trim());
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static CandidateProfileResponse MapCandidateProfileResponse(CandidateProfile candidateProfile)
    {
        return new CandidateProfileResponse(
            candidateProfile.Id,
            candidateProfile.FullName,
            candidateProfile.DateOfBirth,
            CalculateAge(candidateProfile.DateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow)),
            candidateProfile.Province,
            candidateProfile.EducationLevel,
            candidateProfile.Email,
            candidateProfile.IsVisibleToPartnerEmployers,
            candidateProfile.IsAvailableForContact,
            candidateProfile.PhotoUrl,
            candidateProfile.EmailConfirmationSent,
            candidateProfile.CreatedAtUtc);
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly today)
    {
        int age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}

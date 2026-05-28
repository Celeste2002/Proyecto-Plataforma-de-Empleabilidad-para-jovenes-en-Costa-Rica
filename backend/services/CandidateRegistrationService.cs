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
    ILogger<CandidateRegistrationService> logger) : ICandidateRegistrationService
{
    public async Task<CandidateRegistrationResponse> RegisterAsync(
        RegisterCandidateRequest registerCandidateRequest,
        CancellationToken cancellationToken)
    {
        ValidateRegisterCandidateRequest(registerCandidateRequest);

        string normalizedEmail = registerCandidateRequest.Email.Trim().ToLowerInvariant();

        User? existingUser = await userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            throw new RequestValidationException(["Ya existe un candidato registrado con este correo."]);
        }

        DateTime createdAt = DateTime.UtcNow;

        User newUser = new()
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Role = UserRoles.Candidate,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAtUtc = createdAt
        };

        await userRepository.SaveAsync(newUser, cancellationToken);

        CandidateProfile candidateProfile = new()
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            FullName = registerCandidateRequest.FullName.Trim(),
            Age = registerCandidateRequest.Age,
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
                "No se pudo enviar el correo de confirmacion al candidato {CandidateId}. El perfil fue guardado.",
                candidateProfile.Id);
        }

        CandidateProfile savedCandidateProfile = candidateProfile with
        {
            EmailConfirmationSent = emailSent
        };

        string message = emailSent
            ? "Registro completado. Se envio un correo de confirmacion."
            : "Registro completado. No se pudo enviar el correo de confirmacion (SMTP no configurado).";

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

    private static void ValidateRegisterCandidateRequest(RegisterCandidateRequest registerCandidateRequest)
    {
        List<string> validationErrors = [];

        if (string.IsNullOrWhiteSpace(registerCandidateRequest.FullName))
        {
            validationErrors.Add("El nombre es obligatorio.");
        }

        if (registerCandidateRequest.Age is < 18 or > 30)
        {
            validationErrors.Add("La edad debe estar entre 18 y 30 anos.");
        }

        if (!CandidateCatalogs.CostaRicaProvinces.Contains(registerCandidateRequest.Province.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            validationErrors.Add("La provincia debe ser una provincia valida de Costa Rica.");
        }

        if (!CandidateCatalogs.EducationLevels.Contains(registerCandidateRequest.EducationLevel.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            validationErrors.Add("El nivel educativo no es valido.");
        }

        if (!IsValidEmail(registerCandidateRequest.Email))
        {
            validationErrors.Add("El correo electronico no tiene un formato valido.");
        }

        if (validationErrors.Count > 0)
        {
            throw new RequestValidationException(validationErrors);
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
            candidateProfile.Age,
            candidateProfile.Province,
            candidateProfile.EducationLevel,
            candidateProfile.Email,
            candidateProfile.IsVisibleToPartnerEmployers,
            candidateProfile.EmailConfirmationSent,
            candidateProfile.CreatedAtUtc);
    }
}

using System.Net.Mail;
using domain.constants;
using domain.entities;
using Microsoft.Extensions.Logging;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class EmployerRegistrationService(
    IEmployerRepository employerRepository,
    IUserRepository userRepository,
    IEmployerActivationSender employerActivationSender,
    IPasswordHasher passwordHasher,
    ILogger<EmployerRegistrationService> logger) : IEmployerRegistrationService
{
    public async Task<EmployerRegistrationResponse> RegisterAsync(
        RegisterEmployerRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        User? existingUser = await userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            throw new RequestValidationException(["Ya existe una cuenta registrada con este correo."]);
        }

        string passwordHash = passwordHasher.Hash(request.Password);
        DateTime createdAt = DateTime.UtcNow;

        User newUser = new()
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRoles.Employer,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAtUtc = createdAt
        };

        await userRepository.SaveAsync(newUser, cancellationToken);

        EmployerProfile employerProfile = new()
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            CompanyName = request.CompanyName.Trim(),
            LegalId = request.LegalId.Trim(),
            Sector = request.Sector.Trim(),
            ContactName = request.ContactName.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            Location = request.Location.Trim(),
            Email = normalizedEmail,
            Status = EmployerStatus.Active,
            CreatedAtUtc = createdAt,
            ActivationEmailSent = false
        };

        await employerRepository.SaveAsync(employerProfile, cancellationToken);

        try
        {
            await employerActivationSender.SendRegistrationConfirmationAsync(employerProfile, cancellationToken);
        }
        catch (EmailDeliveryException emailDeliveryException)
        {
            logger.LogWarning(emailDeliveryException,
                "No se pudo enviar el correo de confirmación de registro al empleador {Email}. El registro fue guardado.",
                normalizedEmail);
        }

        return new EmployerRegistrationResponse(
            MapResponse(employerProfile),
            "Registro completado. Ya puedes iniciar sesión con tus credenciales.");
    }

    public async Task<EmployerProfileResponse> GetProfileByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        EmployerProfile? profile = await employerRepository.FindByUserIdAsync(userId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        return MapResponse(profile);
    }

    public async Task ActivateAsync(Guid employerProfileId, CancellationToken cancellationToken)
    {
        EmployerProfile? profile = await employerRepository.FindByIdAsync(employerProfileId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException("No se encontró el perfil del empleador.");
        }

        if (profile.Status == EmployerStatus.Active)
        {
            throw new RequestValidationException(["El empleador ya está activo."]);
        }

        await employerRepository.UpdateStatusAsync(employerProfileId, EmployerStatus.Active, cancellationToken);
        await userRepository.SetActiveAsync(profile.UserId, true, cancellationToken);

        try
        {
            await employerActivationSender.SendActivationNotificationAsync(profile, cancellationToken);
            await employerRepository.MarkActivationEmailSentAsync(employerProfileId, cancellationToken);
        }
        catch (EmailDeliveryException emailDeliveryException)
        {
            logger.LogWarning(emailDeliveryException,
                "No se pudo enviar el correo de activación al empleador {EmployerProfileId}. La cuenta fue activada.",
                employerProfileId);
        }
    }

    private static EmployerProfileResponse MapResponse(EmployerProfile profile)
    {
        return new EmployerProfileResponse(
            profile.Id,
            profile.UserId,
            profile.CompanyName,
            profile.LegalId,
            profile.Sector,
            profile.ContactName,
            profile.ContactPhone,
            profile.Location,
            profile.Email,
            profile.Status,
            profile.ActivationEmailSent,
            profile.CreatedAtUtc);
    }

    private static void ValidateRequest(RegisterEmployerRequest request)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            errors.Add("El nombre de la empresa es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.LegalId))
        {
            errors.Add("La cédula jurídica es obligatoria.");
        }

        if (!EmployerSectors.All.Contains(request.Sector?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add("El sector debe ser uno de los sectores válidos.");
        }

        if (string.IsNullOrWhiteSpace(request.ContactName))
        {
            errors.Add("El nombre del contacto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.ContactPhone))
        {
            errors.Add("El teléfono de contacto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            errors.Add("La ubicación es obligatoria.");
        }

        if (!IsValidEmail(request.Email))
        {
            errors.Add("El correo electrónico no tiene un formato válido.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            errors.Add("La contraseña debe tener al menos 8 caracteres.");
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
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
}

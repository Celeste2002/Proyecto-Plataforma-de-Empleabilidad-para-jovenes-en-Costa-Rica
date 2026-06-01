using System.Net;
using System.Net.Mail;
using domain.entities;
using services.exceptions;
using services.interfaces;

namespace infrastructure.email;

public sealed class SmtpEmployerActivationSender(EmailSettings emailSettings) : IEmployerActivationSender
{
    public async Task SendRegistrationConfirmationAsync(
        EmployerProfile employerProfile,
        CancellationToken cancellationToken)
    {
        string senderAddress = GetFirstConfiguredValue(emailSettings.SenderAddress, emailSettings.FromAddress);
        string senderName = GetFirstConfiguredValue(emailSettings.SenderName, emailSettings.FromName);
        string smtpHost = GetFirstConfiguredValue(emailSettings.SmtpHost, emailSettings.Host);
        string smtpUsername = GetFirstConfiguredValue(emailSettings.SmtpUsername, emailSettings.Username);
        string smtpPassword = GetFirstConfiguredValue(emailSettings.SmtpPassword, emailSettings.Password);
        int smtpPort = emailSettings.SmtpPort != 587 ? emailSettings.SmtpPort : emailSettings.Port;

        ValidateSmtpSettings(senderAddress, smtpHost, smtpUsername, smtpPassword);

        using MailMessage mailMessage = new()
        {
            From = new MailAddress(senderAddress, senderName),
            Subject = "Confirmación de registro en Sinergia",
            Body = BuildRegistrationEmailBody(employerProfile),
            IsBodyHtml = false
        };

        mailMessage.To.Add(employerProfile.Email);

        using SmtpClient smtpClient = new(smtpHost, smtpPort)
        {
            EnableSsl = emailSettings.EnableSsl,
            Credentials = new NetworkCredential(smtpUsername, smtpPassword)
        };

        try
        {
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (SmtpException smtpException)
        {
            throw new EmailDeliveryException(
                "El registro fue completado, pero el proveedor SMTP rechazó el envío del correo de confirmación.",
                smtpException);
        }
    }

    public async Task SendActivationNotificationAsync(
        EmployerProfile employerProfile,
        CancellationToken cancellationToken)
    {
        string senderAddress = GetFirstConfiguredValue(emailSettings.SenderAddress, emailSettings.FromAddress);
        string senderName = GetFirstConfiguredValue(emailSettings.SenderName, emailSettings.FromName);
        string smtpHost = GetFirstConfiguredValue(emailSettings.SmtpHost, emailSettings.Host);
        string smtpUsername = GetFirstConfiguredValue(emailSettings.SmtpUsername, emailSettings.Username);
        string smtpPassword = GetFirstConfiguredValue(emailSettings.SmtpPassword, emailSettings.Password);
        int smtpPort = emailSettings.SmtpPort != 587 ? emailSettings.SmtpPort : emailSettings.Port;

        ValidateSmtpSettings(senderAddress, smtpHost, smtpUsername, smtpPassword);

        using MailMessage mailMessage = new()
        {
            From = new MailAddress(senderAddress, senderName),
            Subject = "Tu cuenta de empleador ha sido activada",
            Body = BuildEmailBody(employerProfile),
            IsBodyHtml = false
        };

        mailMessage.To.Add(employerProfile.Email);

        using SmtpClient smtpClient = new(smtpHost, smtpPort)
        {
            EnableSsl = emailSettings.EnableSsl,
            Credentials = new NetworkCredential(smtpUsername, smtpPassword)
        };

        try
        {
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (SmtpException smtpException)
        {
            throw new EmailDeliveryException(
                "La cuenta fue activada, pero el proveedor SMTP rechazó el envío del correo de confirmación.",
                smtpException);
        }
    }

    private static string BuildRegistrationEmailBody(EmployerProfile profile)
    {
        return $"""
        Hola {profile.ContactName},

        Hemos recibido el registro de la empresa "{profile.CompanyName}" en la plataforma Sinergia.

        Sector: {profile.Sector}
        Ubicación: {profile.Location}

        En breve un administrador revisará tu solicitud y recibirás una notificación cuando tu cuenta sea activada.

        Gracias por unirte a Sinergia.
        """;
    }

    private static string BuildEmailBody(EmployerProfile profile)
    {
        return $"""
        Hola {profile.ContactName},

        Tu cuenta de empleador para la empresa "{profile.CompanyName}" ha sido verificada y activada en la plataforma Sinergia.

        Ya puedes iniciar sesión y publicar vacantes para encontrar candidatos jóvenes calificados.

        Sector: {profile.Sector}
        Ubicación: {profile.Location}

        ¡Bienvenido a la plataforma!
        """;
    }

    private static string GetFirstConfiguredValue(string primaryValue, string fallbackValue)
    {
        return string.IsNullOrWhiteSpace(primaryValue) || primaryValue.EndsWith("example.com", StringComparison.OrdinalIgnoreCase)
            ? fallbackValue
            : primaryValue;
    }

    private static void ValidateSmtpSettings(
        string senderAddress,
        string smtpHost,
        string smtpUsername,
        string smtpPassword)
    {
        List<string> missingSettings = [];

        if (string.IsNullOrWhiteSpace(senderAddress))
        {
            missingSettings.Add("Smtp__FromAddress");
        }

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            missingSettings.Add("Smtp__Host");
        }

        if (string.IsNullOrWhiteSpace(smtpUsername))
        {
            missingSettings.Add("Smtp__Username");
        }

        if (string.IsNullOrWhiteSpace(smtpPassword))
        {
            missingSettings.Add("Smtp__Password");
        }

        if (missingSettings.Count > 0)
        {
            throw new EmailDeliveryException(
                $"Faltan variables SMTP obligatorias: {string.Join(", ", missingSettings)}.");
        }
    }
}

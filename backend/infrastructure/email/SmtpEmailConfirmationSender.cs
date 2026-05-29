using System.Net;
using System.Net.Mail;
using domain.entities;
using services.exceptions;
using services.interfaces;

namespace infrastructure.email;

public sealed class SmtpEmailConfirmationSender(EmailSettings emailSettings) : IEmailConfirmationSender
{
    public async Task SendRegistrationConfirmationAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken)
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
            Subject = "Confirmación de registro",
            Body = BuildEmailBody(candidateProfile),
            IsBodyHtml = false
        };

        mailMessage.To.Add(candidateProfile.Email);

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
                "El perfil fue guardado en SQL Server, pero el proveedor SMTP rechazó el envío del correo de confirmación.",
                smtpException);
        }
    }

    private static string BuildEmailBody(CandidateProfile candidateProfile)
    {
        return $"""
        Hola {candidateProfile.FullName},

        Tu perfil de candidato fue creado correctamente en la plataforma de empleabilidad.

        Provincia: {candidateProfile.Province}
        Nivel educativo: {candidateProfile.EducationLevel}

        Tu perfil ya es visible para empleadores aliados.
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

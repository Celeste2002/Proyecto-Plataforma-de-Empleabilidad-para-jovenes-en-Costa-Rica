using System.Net;
using System.Net.Mail;
using services.exceptions;
using services.interfaces;

namespace infrastructure.email;

public sealed class SmtpPasswordResetSender(EmailSettings emailSettings, string frontendUrl) : IPasswordResetSender
{
    private const string ResetPasswordPath = "/restablecer-contrasena";

    public async Task SendPasswordResetAsync(string email, string resetToken, CancellationToken cancellationToken)
    {
        string resetLink = BuildResetLink(resetToken);

        string senderAddress = GetFirstConfiguredValue(emailSettings.SenderAddress, emailSettings.FromAddress);
        string senderName = GetFirstConfiguredValue(emailSettings.SenderName, emailSettings.FromName);
        string smtpHost = GetFirstConfiguredValue(emailSettings.SmtpHost, emailSettings.Host);
        string smtpUsername = GetFirstConfiguredValue(emailSettings.SmtpUsername, emailSettings.Username);
        string smtpPassword = GetFirstConfiguredValue(emailSettings.SmtpPassword, emailSettings.Password);
        int port = emailSettings.SmtpPort != 587 ? emailSettings.SmtpPort : emailSettings.Port;

        ValidateSmtpSettings(senderAddress, smtpHost, smtpUsername, smtpPassword);

        using SmtpClient smtpClient = new(smtpHost, port)
        {
            EnableSsl = emailSettings.EnableSsl,
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
        };

        using MailMessage mailMessage = new()
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = "Recuperación de contraseña - Sinergia",
            Body = $"""
                Hola,

                Recibimos una solicitud para recuperar tu contraseña en Sinergia.

                Haz clic en el siguiente enlace para establecer una nueva contraseña:
                {resetLink}

                Este enlace es valido por 1 hora.

                Si no solicitaste este cambio, puedes ignorar este correo.

                Sinergia - Plataforma de Empleabilidad
                """,
            IsBodyHtml = false,
        };

        mailMessage.To.Add(email);

        try
        {
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (SmtpException smtpException)
        {
            throw new EmailDeliveryException(
                "No se pudo enviar el correo de recuperación de contraseña.",
                smtpException);
        }
    }

    private string BuildResetLink(string resetToken)
    {
        string normalizedFrontendUrl = frontendUrl.TrimEnd('/');
        string encodedResetToken = Uri.EscapeDataString(resetToken);

        return $"{normalizedFrontendUrl}{ResetPasswordPath}?token={encodedResetToken}";
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

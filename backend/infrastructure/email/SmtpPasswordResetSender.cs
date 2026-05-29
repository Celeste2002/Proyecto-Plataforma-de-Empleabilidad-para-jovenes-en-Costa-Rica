using System.Net;
using System.Net.Mail;
using services.exceptions;
using services.interfaces;

namespace infrastructure.email;

public sealed class SmtpPasswordResetSender(EmailSettings emailSettings, string frontendUrl) : IPasswordResetSender
{
    public async Task SendPasswordResetAsync(string email, string resetToken, CancellationToken cancellationToken)
    {
        string resetLink = $"{frontendUrl}/nueva-contrasena?token={resetToken}";

        string host = emailSettings.SmtpHost ?? emailSettings.Host
            ?? throw new InvalidOperationException("SMTP host no configurado.");

        int port = emailSettings.SmtpPort != 587 ? emailSettings.SmtpPort : emailSettings.Port;

        string username = emailSettings.SmtpUsername ?? emailSettings.Username
            ?? throw new InvalidOperationException("SMTP username no configurado.");

        string password = emailSettings.SmtpPassword ?? emailSettings.Password
            ?? throw new InvalidOperationException("SMTP password no configurado.");

        string fromAddress = emailSettings.SenderAddress ?? emailSettings.FromAddress
            ?? throw new InvalidOperationException("Direccion de remitente no configurada.");

        string fromName = emailSettings.SenderName ?? emailSettings.FromName ?? "Plataforma de Empleabilidad";

        using SmtpClient smtpClient = new(host, port)
        {
            EnableSsl = emailSettings.EnableSsl,
            Credentials = new NetworkCredential(username, password),
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
}

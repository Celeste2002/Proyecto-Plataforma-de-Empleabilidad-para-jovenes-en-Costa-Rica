using System.Net;
using System.Net.Mail;
using System.Text;
using domain.entities;
using services.exceptions;
using services.interfaces;

namespace infrastructure.email;

public sealed class SmtpInterviewRequestSender(EmailSettings emailSettings) : IInterviewRequestSender
{
    public async Task SendInterviewRequestAsync(
        EmployerProfile employerProfile,
        Postulacion postulacion,
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
            Subject = $"Solicitud de entrevista: {postulacion.JobTitle}",
            Body = ProfessionalEmailTemplate.BuildInterviewRequest(
                postulacion.CandidateFullName,
                employerProfile.CompanyName,
                postulacion.JobTitle,
                employerProfile.ContactName,
                employerProfile.Email,
                employerProfile.ContactPhone),
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        mailMessage.To.Add(postulacion.CandidateEmail);
        mailMessage.ReplyToList.Add(new MailAddress(employerProfile.Email, employerProfile.ContactName));

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
                "No se pudo enviar el correo de solicitud de entrevista.",
                smtpException);
        }
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

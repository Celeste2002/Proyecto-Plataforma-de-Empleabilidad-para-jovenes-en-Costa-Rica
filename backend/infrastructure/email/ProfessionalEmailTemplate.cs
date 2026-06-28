using System.Net;

namespace infrastructure.email;

internal static class ProfessionalEmailTemplate
{
    private const string BrandName = "Sinergia";
    private const string PrimaryColor = "#0F766E";
    private const string AccentColor = "#14B8A6";
    private const string SurfaceColor = "#F8FAFC";
    private const string TextColor = "#0F172A";
    private const string MutedTextColor = "#475569";

    public static string BuildRegistrationConfirmation(string fullName, string province, string educationLevel)
    {
        string safeFullName = WebUtility.HtmlEncode(fullName);
        string safeProvince = WebUtility.HtmlEncode(province);
        string safeEducationLevel = WebUtility.HtmlEncode(educationLevel);

        return BuildLayout(
            $"Registro confirmado | {BrandName}",
            $"""
            <p style="margin:0 0 16px 0;">Hola <strong>{safeFullName}</strong>,</p>
            <p style="margin:0 0 16px 0;">
                Tu cuenta y tu perfil de candidato fueron creados correctamente en <strong>{BrandName}</strong>.
                Ya puedes acceder a la plataforma y continuar con tu proceso de empleabilidad.
            </p>
            <div style="margin:24px 0;padding:16px;border-radius:14px;background:#F1F5F9;border:1px solid #E2E8F0;">
                <p style="margin:0 0 8px 0;"><strong>Resumen de tu perfil</strong></p>
                <p style="margin:0;color:{MutedTextColor};">Provincia: {safeProvince}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Nivel educativo: {safeEducationLevel}</p>
            </div>
            <p style="margin:0 0 16px 0;">
                Tu perfil ya puede ser consultado por empleadores aliados de la plataforma.
            </p>
            <p style="margin:0;">
                Gracias por confiar en <strong>{BrandName}</strong>.
            </p>
            """);
    }

    public static string BuildPasswordReset(string resetLink, string email)
    {
        string safeResetLink = WebUtility.HtmlEncode(resetLink);
        string safeEmail = WebUtility.HtmlEncode(email);

        return BuildLayout(
            $"Recuperacion de contrasena | {BrandName}",
            $"""
            <p style="margin:0 0 16px 0;">Hola,</p>
            <p style="margin:0 0 16px 0;">
                Recibimos una solicitud para restablecer el acceso a tu cuenta en <strong>{BrandName}</strong>.
            </p>
            <div style="margin:24px 0;padding:16px;border-radius:14px;background:#F1F5F9;border:1px solid #E2E8F0;">
                <p style="margin:0 0 8px 0;"><strong>Solicitud de seguridad</strong></p>
                <p style="margin:0;color:{MutedTextColor};">Cuenta: {safeEmail}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Vigencia del enlace: 1 hora</p>
            </div>
            <p style="margin:0 0 20px 0;">
                Haz clic en el siguiente boton para crear una nueva contrasena:
            </p>
            <div style="text-align:center;margin:0 0 28px 0;">
                <a href="{safeResetLink}"
                   style="display:inline-block;background:{PrimaryColor};color:#FFFFFF;text-decoration:none;font-weight:700;padding:14px 28px;border-radius:10px;">
                    Restablecer contrasena
                </a>
            </div>
            <p style="margin:0 0 16px 0;">
                Si no solicitaste este cambio, puedes ignorar este mensaje y tu contrasena seguira igual.
            </p>
            <p style="margin:0;">
                Equipo de <strong>{BrandName}</strong>
            </p>
            <p style="margin:12px 0 0 0;color:{MutedTextColor};font-size:13px;">
                Si el boton no funciona, copia y pega este enlace en tu navegador:<br />
                <a href="{safeResetLink}" style="color:{AccentColor};word-break:break-all;">{safeResetLink}</a>
            </p>
            """);
    }

    public static string BuildInterviewRequest(
        string candidateFullName,
        string companyName,
        string jobTitle,
        string employerContactName,
        string employerEmail,
        string employerPhone)
    {
        string safeCandidateFullName = WebUtility.HtmlEncode(candidateFullName);
        string safeCompanyName = WebUtility.HtmlEncode(companyName);
        string safeJobTitle = WebUtility.HtmlEncode(jobTitle);
        string safeEmployerContactName = WebUtility.HtmlEncode(employerContactName);
        string safeEmployerEmail = WebUtility.HtmlEncode(employerEmail);
        string safeEmployerPhone = WebUtility.HtmlEncode(employerPhone);

        return BuildLayout(
            $"Solicitud de entrevista | {BrandName}",
            $"""
            <p style="margin:0 0 16px 0;">Hola <strong>{safeCandidateFullName}</strong>,</p>
            <p style="margin:0 0 16px 0;">
                <strong>{safeCompanyName}</strong> quiere coordinar una entrevista contigo para la vacante
                <strong>{safeJobTitle}</strong>.
            </p>
            <div style="margin:24px 0;padding:16px;border-radius:14px;background:#F1F5F9;border:1px solid #E2E8F0;">
                <p style="margin:0 0 8px 0;"><strong>Contacto del empleador</strong></p>
                <p style="margin:0;color:{MutedTextColor};">Persona de contacto: {safeEmployerContactName}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Correo: {safeEmployerEmail}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Telefono: {safeEmployerPhone}</p>
            </div>
            <p style="margin:0 0 16px 0;">
                Para continuar el proceso, responde directamente a este correo o escribe al contacto indicado.
                La coordinacion de fecha, hora y modalidad se realizara fuera de la plataforma.
            </p>
            <p style="margin:0;">
                Equipo de <strong>{BrandName}</strong>
            </p>
            """,
            $"""
            Puedes responder este correo para coordinar directamente con el empleador.
            {BrandName} no usa mensajeria interna para coordinar esta etapa del proceso.
            """);
    }

    private static string BuildLayout(string subject, string content, string? footer = null)
    {
        string footerContent = footer ??
            $"Este es un mensaje automatico generado por {BrandName}. Por favor, no respondas a este correo.";

        return $"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{WebUtility.HtmlEncode(subject)}</title>
        </head>
        <body style="margin:0;padding:0;background:#E2E8F0;font-family:Arial,Helvetica,sans-serif;color:{TextColor};">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#E2E8F0;padding:32px 16px;">
                <tr>
                    <td align="center">
                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:640px;background:#FFFFFF;border-radius:20px;overflow:hidden;box-shadow:0 18px 40px rgba(15,23,42,0.12);">
                            <tr>
                                <td style="background:linear-gradient(135deg,{PrimaryColor} 0%, {AccentColor} 100%);padding:30px 32px;color:#FFFFFF;">
                                    <p style="margin:0;font-size:13px;letter-spacing:0.08em;text-transform:uppercase;opacity:0.9;">Plataforma de empleabilidad</p>
                                    <h1 style="margin:8px 0 0 0;font-size:28px;line-height:1.2;">{BrandName}</h1>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding:32px;background:{SurfaceColor};">
                                    <div style="background:#FFFFFF;border:1px solid #E2E8F0;border-radius:16px;padding:28px 24px;line-height:1.7;font-size:16px;">
                                        {content}
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding:0 32px 28px 32px;background:{SurfaceColor};">
                                    <p style="margin:0;color:{MutedTextColor};font-size:13px;line-height:1.6;text-align:center;">
                                        {footerContent}
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }
}

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
            $"Recuperación de contraseña | {BrandName}",
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
                Haz clic en el siguiente botón para crear una nueva contraseña:
            </p>
            <div style="text-align:center;margin:0 0 28px 0;">
                <a href="{safeResetLink}"
                   style="display:inline-block;background:{PrimaryColor};color:#FFFFFF;text-decoration:none;font-weight:700;padding:14px 28px;border-radius:10px;">
                    Restablecer contraseña
                </a>
            </div>
            <p style="margin:0 0 16px 0;">
                Si no solicitaste este cambio, puedes ignorar este mensaje y tu contraseña seguirá igual.
            </p>
            <p style="margin:0;">
                Equipo de <strong>{BrandName}</strong>
            </p>
            <p style="margin:12px 0 0 0;color:{MutedTextColor};font-size:13px;">
                Si el botón no funciona, copia y pega este enlace en tu navegador:<br />
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
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Teléfono: {safeEmployerPhone}</p>
            </div>
            <p style="margin:0 0 16px 0;">
                Para continuar el proceso, responde directamente al correo de la persona de contacto.
                La coordinación de fecha, hora y modalidad se realizará fuera de la plataforma.
            </p>
            <p style="margin:0;">
                Equipo de <strong>{BrandName}</strong>
            </p>
            """);
    }

    public static string BuildPostulacionSuggestion(
        string candidateFullName,
        string companyName,
        string jobTitle,
        string province,
        string modality,
        string experienceLevel,
        string? message,
        string employerContactName,
        string employerEmail,
        string employerPhone)
    {
        string safeCandidateFullName = WebUtility.HtmlEncode(candidateFullName);
        string safeCompanyName = WebUtility.HtmlEncode(companyName);
        string safeJobTitle = WebUtility.HtmlEncode(jobTitle);
        string safeProvince = WebUtility.HtmlEncode(province);
        string safeModality = WebUtility.HtmlEncode(modality);
        string safeExperienceLevel = WebUtility.HtmlEncode(experienceLevel);
        string safeEmployerContactName = WebUtility.HtmlEncode(employerContactName);
        string safeEmployerEmail = WebUtility.HtmlEncode(employerEmail);
        string safeEmployerPhone = WebUtility.HtmlEncode(employerPhone);
        string messageBlock = string.IsNullOrWhiteSpace(message)
            ? string.Empty
            : $"""<p style="margin:16px 0 0 0;color:{MutedTextColor};"><strong>Mensaje de la empresa:</strong> {WebUtility.HtmlEncode(message)}</p>""";

        return BuildLayout(
            $"Sugerencia de postulación | {BrandName}",
            $"""
            <p style="margin:0 0 16px 0;">Hola <strong>{safeCandidateFullName}</strong>,</p>
            <p style="margin:0 0 16px 0;">
                <strong>{safeCompanyName}</strong> revisó tu perfil y te sugiere postularte a la vacante
                <strong>{safeJobTitle}</strong>.
            </p>
            <div style="margin:24px 0;padding:16px;border-radius:14px;background:#F1F5F9;border:1px solid #E2E8F0;">
                <p style="margin:0 0 8px 0;"><strong>Detalles de la vacante</strong></p>
                <p style="margin:0;color:{MutedTextColor};">Provincia: {safeProvince}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Modalidad: {safeModality}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Experiencia requerida: {safeExperienceLevel}</p>
                {messageBlock}
            </div>
            <p style="margin:0 0 16px 0;">
                Ingresa a la plataforma y ve a la sección "Postulaciones enviadas por empresas" para postularte
                con un solo clic.
            </p>
            <div style="margin:24px 0;padding:16px;border-radius:14px;background:#F1F5F9;border:1px solid #E2E8F0;">
                <p style="margin:0 0 8px 0;"><strong>Contacto del empleador</strong></p>
                <p style="margin:0;color:{MutedTextColor};">Persona de contacto: {safeEmployerContactName}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Correo: {safeEmployerEmail}</p>
                <p style="margin:6px 0 0 0;color:{MutedTextColor};">Teléfono: {safeEmployerPhone}</p>
            </div>
            <p style="margin:0;">
                Equipo de <strong>{BrandName}</strong>
            </p>
            """);
    }

    public static string BuildPostulacionDeclined(
        string candidateFullName,
        string companyName,
        string jobTitle)
    {
        string safeCandidateFullName = WebUtility.HtmlEncode(candidateFullName);
        string safeCompanyName = WebUtility.HtmlEncode(companyName);
        string safeJobTitle = WebUtility.HtmlEncode(jobTitle);

        return BuildLayout(
            $"Actualizacion de postulacion | {BrandName}",
            $"""
            <p style="margin:0 0 16px 0;">Hola <strong>{safeCandidateFullName}</strong>,</p>
            <p style="margin:0 0 16px 0;">
                Gracias por tu interes en la vacante <strong>{safeJobTitle}</strong> de
                <strong>{safeCompanyName}</strong>.
            </p>
            <p style="margin:0 0 16px 0;">
                Despues de revisar la informacion de tu postulacion, la empresa ha decidido continuar
                el proceso con perfiles que se ajustan mejor a los requerimientos actuales del puesto.
            </p>
            <p style="margin:0 0 16px 0;">
                Agradecemos el tiempo que dedicaste a postularte y te animamos a seguir explorando
                nuevas oportunidades dentro de la plataforma.
            </p>
            <p style="margin:0;">
                Equipo de <strong>{BrandName}</strong>
            </p>
            """);
    }

    public static string BuildVacanteFilled(
        string candidateFullName,
        string companyName,
        string jobTitle)
    {
        string safeCandidateFullName = WebUtility.HtmlEncode(candidateFullName);
        string safeCompanyName = WebUtility.HtmlEncode(companyName);
        string safeJobTitle = WebUtility.HtmlEncode(jobTitle);

        return BuildLayout(
            $"Vacante cubierta | {BrandName}",
            $"""
            <p style="margin:0 0 16px 0;">Hola <strong>{safeCandidateFullName}</strong>,</p>
            <p style="margin:0 0 16px 0;">
                Te informamos que la vacante <strong>{safeJobTitle}</strong> de
                <strong>{safeCompanyName}</strong> fue desactivada porque ya fue llenada.
            </p>
            <p style="margin:0 0 16px 0;">
                Gracias por haber participado en el proceso. Tu postulacion queda cerrada para esta
                oportunidad, pero puedes continuar revisando nuevas vacantes disponibles en la plataforma.
            </p>
            <p style="margin:0;">
                Equipo de <strong>{BrandName}</strong>
            </p>
            """);
    }

    private static string BuildLayout(string subject, string content)
    {
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
                                        Este es un mensaje automático generado por {BrandName}. Por favor, no respondas a este correo.
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

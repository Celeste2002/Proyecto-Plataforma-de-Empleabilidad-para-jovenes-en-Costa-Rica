namespace infrastructure.email;

public sealed class EmailSettings
{
    public string Provider { get; init; } = "Smtp";

    public string SenderName { get; init; } = "Plataforma de Empleabilidad";

    public string SenderAddress { get; init; } = "no-reply@example.com";

    public string SmtpHost { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public int SmtpPort { get; init; } = 587;

    public int Port { get; init; } = 587;

    public string SmtpUsername { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string SmtpPassword { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FromName { get; init; } = string.Empty;

    public string FromAddress { get; init; } = string.Empty;

    public bool EnableSsl { get; init; } = true;
}

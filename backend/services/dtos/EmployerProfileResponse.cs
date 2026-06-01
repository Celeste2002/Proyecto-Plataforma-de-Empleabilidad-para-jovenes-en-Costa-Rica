namespace services.dtos;

public sealed record EmployerProfileResponse(
    Guid Id,
    Guid UserId,
    string CompanyName,
    string LegalId,
    string Sector,
    string ContactName,
    string ContactPhone,
    string Location,
    string Email,
    string Status,
    bool ActivationEmailSent,
    DateTime CreatedAtUtc);

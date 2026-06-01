namespace services.dtos;

public sealed record RegisterEmployerRequest(
    string CompanyName,
    string LegalId,
    string Sector,
    string ContactName,
    string ContactPhone,
    string Location,
    string Email,
    string Password);

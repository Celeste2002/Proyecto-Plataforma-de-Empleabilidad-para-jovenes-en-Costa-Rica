namespace services.dtos;

public sealed record SugerenciaRecibidaResponse(
    Guid Id,
    Guid VacanteId,
    string JobTitle,
    string CompanyName,
    string Province,
    string? Message,
    DateTime CreatedAtUtc,
    bool VacanteIsActive,
    bool AlreadyApplied);

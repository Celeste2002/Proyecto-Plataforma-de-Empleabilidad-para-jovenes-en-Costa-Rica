namespace services.dtos;

public sealed record SugerenciaPostulacionResponse(
    Guid Id,
    Guid VacanteId,
    string JobTitle,
    string CompanyName,
    Guid CandidateProfileId,
    string? Message,
    DateTime CreatedAtUtc);

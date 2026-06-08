namespace services.dtos;

public sealed record PostulacionResponse(
    Guid Id,
    Guid VacanteId,
    string JobTitle,
    string CompanyName,
    string Province,
    string Status,
    DateTime AppliedAt,
    DateTime UpdatedAtUtc);

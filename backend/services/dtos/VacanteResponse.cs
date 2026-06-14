namespace services.dtos;

public sealed record VacanteResponse(
    Guid Id,
    string JobTitle,
    string CompanyName,
    string Province,
    string Sector,
    string Modality,
    string ExperienceLevel,
    string? Description,
    bool IsActive,
    DateTime PublishedAt);

namespace services.dtos;

public sealed record CreateVacanteRequest(
    string JobTitle,
    string Province,
    string Sector,
    string Modality,
    string ExperienceLevel,
    string? Description,
    string? Requirements,
    string? SalaryRange);

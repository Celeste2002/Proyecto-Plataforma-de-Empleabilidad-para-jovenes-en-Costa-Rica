namespace services.dtos;

public sealed record UpdateVacanteRequest(
    string? Description,
    string? Requirements,
    string? SalaryRange);

namespace services.dtos;

public sealed record AddExperienciaLaboralRequest(
    string Empresa,
    string Cargo,
    DateOnly FechaInicio,
    DateOnly? FechaFin,
    bool EsTrabajoActual,
    string? Descripcion);

public sealed record ExperienciaLaboralResponse(
    Guid Id,
    string Empresa,
    string Cargo,
    DateOnly FechaInicio,
    DateOnly? FechaFin,
    bool EsTrabajoActual,
    string? Descripcion);

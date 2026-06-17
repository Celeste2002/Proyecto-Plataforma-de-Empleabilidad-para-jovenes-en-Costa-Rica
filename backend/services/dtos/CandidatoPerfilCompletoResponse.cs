namespace services.dtos;

public sealed record CandidatoPerfilCompletoResponse(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    int Age,
    string Province,
    string EducationLevel,
    string Email,
    bool IsAvailableForContact,
    bool IsVisibleToPartnerEmployers,
    string? PhotoUrl,
    IReadOnlyCollection<ExperienciaLaboralResponse> Experiencias,
    IReadOnlyCollection<HabilidadResponse> Habilidades,
    IReadOnlyCollection<CursoCompletadoResponse> Cursos);

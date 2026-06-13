namespace services.dtos;

public sealed record AddCursoCompletadoRequest(
    string NombreCurso,
    string Institucion,
    DateOnly FechaCompletado,
    bool EsDePlataforma);

public sealed record CursoCompletadoResponse(
    Guid Id,
    string NombreCurso,
    string Institucion,
    DateOnly FechaCompletado,
    bool EsDePlataforma);

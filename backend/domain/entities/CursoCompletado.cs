namespace domain.entities;

public sealed record CursoCompletado
{
    public required Guid Id { get; init; }

    public required Guid CandidateProfileId { get; init; }

    public required string NombreCurso { get; init; }

    public required string Institucion { get; init; }

    public required DateOnly FechaCompletado { get; init; }

    public bool EsDePlataforma { get; init; }
}

namespace domain.entities;

public sealed record ExperienciaLaboral
{
    public required Guid Id { get; init; }

    public required Guid CandidateProfileId { get; init; }

    public required string Empresa { get; init; }

    public required string Cargo { get; init; }

    public required DateOnly FechaInicio { get; init; }

    public DateOnly? FechaFin { get; init; }

    public bool EsTrabajoActual { get; init; }

    public string? Descripcion { get; init; }
}

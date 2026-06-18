namespace domain.entities;

public sealed record Habilidad
{
    public required Guid Id { get; init; }

    public required Guid CandidateProfileId { get; init; }

    public required string Nombre { get; init; }
}

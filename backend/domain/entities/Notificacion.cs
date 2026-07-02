namespace domain.entities;

public sealed class Notificacion
{
    public required Guid Id { get; init; }
    public Guid? EmployerProfileId { get; init; }
    public Guid? CandidateProfileId { get; init; }
    public required Guid PostulacionId { get; init; }
    public required Guid VacanteId { get; init; }
    public required string Message { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

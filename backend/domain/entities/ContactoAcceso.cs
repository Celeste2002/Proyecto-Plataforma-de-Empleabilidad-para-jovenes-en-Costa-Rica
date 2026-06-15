namespace domain.entities;

public sealed class ContactoAcceso
{
    public required Guid Id { get; init; }
    public required Guid EmployerProfileId { get; init; }
    public required Guid PostulacionId { get; init; }
    public required string CandidateEmail { get; init; }
    public DateTime AccessedAtUtc { get; init; }
}

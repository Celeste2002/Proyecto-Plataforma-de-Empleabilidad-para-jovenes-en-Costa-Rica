namespace domain.entities;

public sealed class Mensaje
{
    public required Guid Id { get; init; }
    public required Guid PostulacionId { get; init; }
    public required Guid SenderEmployerProfileId { get; init; }
    public required Guid RecipientCandidateProfileId { get; init; }
    public required string Body { get; init; }
    public DateTime SentAtUtc { get; init; }
    public bool IsReadByCandidate { get; init; }

    // Campos de enriquecimiento — populados por JOIN en GetByRecipientCandidateProfileIdAsync
    public string SenderCompanyName { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
}

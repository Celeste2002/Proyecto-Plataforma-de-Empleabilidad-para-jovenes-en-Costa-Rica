namespace domain.entities;

public sealed class Postulacion
{
    public required Guid Id { get; init; }
    public required Guid VacanteId { get; init; }
    public required Guid CandidateProfileId { get; init; }
    public required string Status { get; init; }
    public DateTime AppliedAt { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string Province { get; init; } = string.Empty;

    // Campos de enriquecimiento — populados por JOIN en GetByVacanteIdWithCandidateAsync
    public string CandidateFullName       { get; init; } = string.Empty;
    public string CandidateEmail          { get; init; } = string.Empty;
    public string CandidateProvince       { get; init; } = string.Empty;
    public string CandidateEducationLevel { get; init; } = string.Empty;
    public int    CandidateAge            { get; init; }
}

namespace services.dtos;

public sealed record PostulacionSummaryResponse(
    Guid Id,
    Guid CandidateProfileId,
    string CandidateFullName,
    string Status,
    DateTime AppliedAt,
    DateTime UpdatedAtUtc);

namespace services.dtos;

public sealed record PostulacionConCandidatoResponse(
    Guid PostulacionId,
    string Status,
    DateTime AppliedAt,
    DateTime UpdatedAtUtc,
    Guid CandidateProfileId,
    string CandidateFullName,
    string CandidateEmail,
    string CandidateProvince,
    string CandidateEducationLevel,
    int CandidateAge);

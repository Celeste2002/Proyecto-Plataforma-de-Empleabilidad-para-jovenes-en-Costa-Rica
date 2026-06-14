namespace services.dtos;

public sealed record PostulacionDetailResponse(
    Guid Id,
    Guid VacanteId,
    string JobTitle,
    string Status,
    DateTime AppliedAt,
    DateTime UpdatedAtUtc,
    Guid CandidateProfileId,
    string CandidateFullName,
    string CandidateEmail,
    string CandidateProvince,
    string CandidateEducationLevel,
    DateOnly CandidateDateOfBirth,
    int CandidateAge);

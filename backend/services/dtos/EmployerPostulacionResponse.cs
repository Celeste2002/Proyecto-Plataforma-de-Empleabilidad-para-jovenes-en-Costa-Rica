namespace services.dtos;

public sealed record EmployerPostulacionResponse(
    Guid Id,
    Guid VacanteId,
    Guid CandidateProfileId,
    string CandidateFullName,
    string CandidateEmail,
    string CandidateProvince,
    string CandidateEducationLevel,
    string Status,
    DateTime AppliedAt,
    DateTime UpdatedAtUtc);

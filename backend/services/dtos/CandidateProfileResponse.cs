namespace services.dtos;

public sealed record CandidateProfileResponse(
    Guid Id,
    string FullName,
    int Age,
    string Province,
    string EducationLevel,
    string Email,
    bool IsVisibleToPartnerEmployers,
    bool EmailConfirmationSent,
    DateTime CreatedAtUtc);

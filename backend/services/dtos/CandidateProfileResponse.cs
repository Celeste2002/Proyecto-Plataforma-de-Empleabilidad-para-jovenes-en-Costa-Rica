namespace services.dtos;

public sealed record CandidateProfileResponse(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    int Age,
    string Province,
    string EducationLevel,
    string Email,
    bool IsVisibleToPartnerEmployers,
    bool IsAvailableForContact,
    string? PhotoUrl,
    bool EmailConfirmationSent,
    DateTime CreatedAtUtc);

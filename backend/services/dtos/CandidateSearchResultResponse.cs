namespace services.dtos;

public sealed record CandidateSearchResultResponse(
    Guid Id,
    string FullName,
    int Age,
    string Province,
    string EducationLevel,
    string Email,
    bool IsAvailableForContact,
    string? PhotoUrl,
    decimal ExperienceYears,
    bool HasAppliedToYourVacantes);

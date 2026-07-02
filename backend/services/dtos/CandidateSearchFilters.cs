namespace services.dtos;

public sealed record CandidateSearchFilters(
    string? SkillKeyword,
    string? Province,
    string? EducationLevel,
    decimal? MinExperienceYears,
    bool? IsAvailableForContact);

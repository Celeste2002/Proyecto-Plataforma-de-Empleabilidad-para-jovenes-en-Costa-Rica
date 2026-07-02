namespace domain.entities;

public sealed class CandidateSearchResult
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string Province { get; init; }
    public required string EducationLevel { get; init; }
    public required string Email { get; init; }
    public bool IsAvailableForContact { get; init; }
    public string? PhotoUrl { get; init; }
    public decimal ExperienceYears { get; init; }
    public bool HasAppliedToYourVacantes { get; init; }
}

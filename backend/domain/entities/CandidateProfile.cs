namespace domain.entities;

public sealed record CandidateProfile
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string FullName { get; init; }

    public required DateOnly DateOfBirth { get; init; }

    public required string Province { get; init; }

    public required string EducationLevel { get; init; }

    // Populado desde Users.Email via JOIN — no se almacena en CandidateProfiles
    public required string Email { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public bool IsVisibleToPartnerEmployers { get; init; } = true;

    public bool IsAvailableForContact { get; init; } = true;

    public string? PhotoUrl { get; init; }

    // Populado desde Users.EmailConfirmed via JOIN — no se almacena en CandidateProfiles
    public bool EmailConfirmationSent { get; init; }
}

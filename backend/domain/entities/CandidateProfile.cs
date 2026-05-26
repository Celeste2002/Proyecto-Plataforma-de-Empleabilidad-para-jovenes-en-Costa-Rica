namespace domain.entities;

public sealed record CandidateProfile
{
    public required Guid Id { get; init; }

    public required string FullName { get; init; }

    public required int Age { get; init; }

    public required string Province { get; init; }

    public required string EducationLevel { get; init; }

    public required string Email { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public bool IsVisibleToPartnerEmployers { get; init; } = true;

    public bool EmailConfirmationSent { get; init; }
}

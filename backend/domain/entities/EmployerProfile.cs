namespace domain.entities;

public sealed record EmployerProfile
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string CompanyName { get; init; }

    public required string LegalId { get; init; }

    public required string Sector { get; init; }

    public required string ContactName { get; init; }

    public required string ContactPhone { get; init; }

    public required string Location { get; init; }

    // Populado desde Users.Email via JOIN
    public required string Email { get; init; }

    public required string Status { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    // Populado desde Users.EmailConfirmed via JOIN
    public bool ActivationEmailSent { get; init; }
}

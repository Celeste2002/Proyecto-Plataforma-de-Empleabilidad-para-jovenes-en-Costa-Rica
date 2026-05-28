namespace domain.entities;

public sealed record User
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? PasswordHash { get; init; }
    public string? PasswordResetToken { get; init; }
    public DateTime? PasswordResetTokenExpiresAtUtc { get; init; }
    public required string Role { get; init; }
    public bool IsActive { get; init; } = true;
    public bool EmailConfirmed { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}

namespace domain.entities;

public sealed class Vacante
{
    public required Guid Id { get; init; }
    public required Guid EmployerProfileId { get; init; }
    public required string JobTitle { get; init; }
    public required string Province { get; init; }
    public required string Sector { get; init; }
    public required string Modality { get; init; }
    public required string ExperienceLevel { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime PublishedAt { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string CompanyName { get; init; } = string.Empty;
}

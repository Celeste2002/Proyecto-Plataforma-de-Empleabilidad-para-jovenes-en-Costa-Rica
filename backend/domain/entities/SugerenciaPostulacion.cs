namespace domain.entities;

public sealed class SugerenciaPostulacion
{
    public required Guid Id { get; init; }
    public required Guid VacanteId { get; init; }
    public required Guid CandidateProfileId { get; init; }
    public string? Message { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string Province { get; init; } = string.Empty;
    public bool VacanteIsActive { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string EmployerContactName { get; init; } = string.Empty;
    public bool AlreadyApplied { get; init; }
}

namespace services.dtos;

public sealed record CreateSugerenciaPostulacionRequest(
    Guid CandidateProfileId,
    Guid VacanteId,
    string? Message);

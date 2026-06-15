namespace services.dtos;

public sealed record VacanteConPostulantesResponse(
    Guid VacanteId,
    string JobTitle,
    string Province,
    string Sector,
    string Modality,
    bool IsActive,
    DateTime PublishedAt,
    IReadOnlyCollection<PostulacionConCandidatoResponse> Postulantes);

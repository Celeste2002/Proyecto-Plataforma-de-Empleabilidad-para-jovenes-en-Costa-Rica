namespace services.dtos;

public sealed record AdminReportResponse(
    int TotalUsers,
    int TotalCandidates,
    int TotalEmployers,
    int TotalAdministrators,
    int TotalVacantes,
    int ActiveVacantes,
    int ClosedVacantes,
    int TotalPostulaciones,
    int CandidatesWithPostulaciones,
    int VacantesWithPostulaciones,
    IReadOnlyCollection<PostulacionStatusStat> PostulacionesByStatus,
    IReadOnlyCollection<ProvinceStat> CandidatesByProvince,
    IReadOnlyCollection<ProvinceStat> VacantesByProvince,
    int TotalMicrocursos,
    DateTime GeneratedAt);

public sealed record PostulacionStatusStat(string Status, int Count);

public sealed record ProvinceStat(string Province, int Count);

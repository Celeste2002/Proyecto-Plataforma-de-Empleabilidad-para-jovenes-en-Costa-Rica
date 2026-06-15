namespace services.dtos;

public sealed record UpdatePostulacionStatusResponse(
    Guid PostulacionId,
    string OldStatus,
    string NewStatus,
    DateTime UpdatedAtUtc);

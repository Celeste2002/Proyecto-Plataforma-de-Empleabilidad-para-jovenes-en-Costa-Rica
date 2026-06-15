namespace services.dtos;

public sealed record NotificacionResponse(
    Guid Id,
    Guid PostulacionId,
    Guid VacanteId,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc);

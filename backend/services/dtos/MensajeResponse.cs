namespace services.dtos;

public sealed record MensajeResponse(
    Guid Id,
    Guid PostulacionId,
    Guid SenderEmployerProfileId,
    string SenderCompanyName,
    string JobTitle,
    string Body,
    DateTime SentAtUtc,
    bool IsReadByCandidate);

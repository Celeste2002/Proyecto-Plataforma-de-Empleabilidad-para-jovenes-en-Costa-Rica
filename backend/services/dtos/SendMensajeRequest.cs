namespace services.dtos;

public sealed record SendMensajeRequest(
    Guid PostulacionId,
    string Body);

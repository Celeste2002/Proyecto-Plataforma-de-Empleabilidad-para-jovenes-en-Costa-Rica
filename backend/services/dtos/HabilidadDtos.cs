namespace services.dtos;

public sealed record AddHabilidadRequest(string Nombre);

public sealed record HabilidadResponse(Guid Id, string Nombre);

namespace services.dtos;

public sealed record MicroCursoResponse(
    Guid Id,
    string Titulo,
    string Descripcion,
    string Area,
    int DuracionHoras,
    string EntidadProveedora,
    string TipoProveedor,
    bool OtorgaCertificacion,
    string? EnlaceUrl,
    int CantidadValidaciones,
    IReadOnlyCollection<string> Habilidades,
    int Coincidencias,
    IReadOnlyCollection<string> HabilidadesCoincidentes);


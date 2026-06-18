namespace domain.entities;

public sealed record MicroCurso
{
    public required Guid Id { get; init; }
    public required string Titulo { get; init; }
    public required string Descripcion { get; init; }
    public required string Area { get; init; }
    public required int DuracionHoras { get; init; }
    public required string EntidadProveedora { get; init; }
    public required string TipoProveedor { get; init; }
    public required bool OtorgaCertificacion { get; init; }
    public required int CantidadValidaciones { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public IReadOnlyCollection<string> Habilidades { get; init; } = [];
}


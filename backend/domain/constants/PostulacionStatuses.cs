namespace domain.constants;

public static class PostulacionStatuses
{
    public const string Enviada = "Enviada";
    public const string EnRevision = "En revisión";
    public const string EntrevistaSolicitada = "Entrevista solicitada";
    public const string EntrevistaProgramada = "Entrevista programada";
    public const string Descartado = "Descartado";
    public const string Finalizada = "Finalizada";

    public static readonly IReadOnlyCollection<string> EmployerSettable =
    [
        EnRevision,
        EntrevistaProgramada,
        Descartado
    ];

    public static readonly IReadOnlyCollection<string> All =
    [
        Enviada,
        EnRevision,
        EntrevistaSolicitada,
        EntrevistaProgramada,
        Descartado,
        Finalizada
    ];
}

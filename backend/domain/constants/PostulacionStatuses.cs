namespace domain.constants;

public static class PostulacionStatuses
{
    public const string Enviada = "Enviada";
    public const string Vista = "Vista";
    public const string Entrevista = "Entrevista";
    public const string EnRevision = "En revisión";
    public const string EntrevistaSolicitada = "Entrevista solicitada";
    public const string Finalizada = "Finalizada";

    public static readonly IReadOnlyCollection<string> All =
    [
        Enviada,
        Vista,
        Entrevista,
        EnRevision,
        EntrevistaSolicitada,
        Finalizada
    ];
}

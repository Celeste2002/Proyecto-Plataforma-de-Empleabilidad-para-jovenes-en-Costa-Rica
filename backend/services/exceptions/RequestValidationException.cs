namespace services.exceptions;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(IReadOnlyCollection<string> validationErrors)
        : base("La solicitud contiene datos invalidos.")
    {
        ValidationErrors = validationErrors;
    }

    public IReadOnlyCollection<string> ValidationErrors { get; }
}

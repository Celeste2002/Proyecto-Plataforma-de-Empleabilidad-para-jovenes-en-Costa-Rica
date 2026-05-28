namespace services.exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
}

namespace services.exceptions;

public sealed class AuthenticationException(string message) : Exception(message)
{
}

namespace services.dtos;

public sealed record LoginResponse(
    string Token,
    string Role,
    Guid UserId,
    string Email);

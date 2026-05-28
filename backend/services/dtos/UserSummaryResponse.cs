namespace services.dtos;

public sealed record UserSummaryResponse(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAtUtc);

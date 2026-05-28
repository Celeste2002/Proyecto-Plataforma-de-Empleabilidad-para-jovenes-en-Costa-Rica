namespace services.dtos;

public sealed record ResetPasswordRequest(string Token, string NewPassword);

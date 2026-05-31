namespace services.dtos;

public sealed record UpdateCandidatePasswordRequest(
    string CurrentPassword,
    string NewPassword);

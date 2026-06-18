namespace services.dtos;

public sealed record UpdateCandidateProfileRequest(
    string FullName,
    DateOnly DateOfBirth,
    string Province,
    string EducationLevel,
    string? PhotoUrl);

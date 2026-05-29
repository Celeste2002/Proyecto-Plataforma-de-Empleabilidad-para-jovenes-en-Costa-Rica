namespace services.dtos;

public sealed record RegisterCandidateRequest(
    string FullName,
    DateOnly DateOfBirth,
    string Province,
    string EducationLevel,
    string Email,
    string Password);

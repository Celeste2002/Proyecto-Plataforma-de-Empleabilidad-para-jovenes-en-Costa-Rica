namespace services.dtos;

public sealed record RegisterCandidateRequest(
    string FullName,
    int Age,
    string Province,
    string EducationLevel,
    string Email);

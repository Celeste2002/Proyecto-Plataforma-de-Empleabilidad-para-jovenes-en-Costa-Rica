namespace services.dtos;

public sealed record EmployerRegistrationResponse(
    EmployerProfileResponse EmployerProfile,
    string Message);

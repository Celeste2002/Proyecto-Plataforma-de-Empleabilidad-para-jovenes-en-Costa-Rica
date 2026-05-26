namespace services.dtos;

public sealed record CandidateRegistrationResponse(
    CandidateProfileResponse CandidateProfile,
    string ConfirmationMessage);

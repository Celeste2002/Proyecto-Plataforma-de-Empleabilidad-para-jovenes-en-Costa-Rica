namespace domain.constants;

public static class UserRoles
{
    public const string Candidate = "CANDIDATE";
    public const string Employer = "EMPLOYER";
    public const string Administrator = "ADMINISTRATOR";

    public static readonly IReadOnlyCollection<string> All =
        [Candidate, Employer, Administrator];
}

namespace FitnessClub.Api.Auth;
public static class Roles
{
    public const string User = "User";
    public const string Administrator = "Administrator";
    public static readonly string[] All = { User, Administrator };
}

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string MemberOrAdmin = "MemberOrAdmin";
}

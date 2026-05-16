namespace CateringWebsite.Data;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Caretaker = "Caretaker";
    public const string User = "User";

    public static readonly string[] All = [Admin, Caretaker, User];
}

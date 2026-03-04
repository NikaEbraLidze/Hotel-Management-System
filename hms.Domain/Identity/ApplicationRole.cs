namespace hms.Domain.Identity
{
    public enum AppRole
    {
        Admin = 1,
        Manager = 2,
        Guest = 3
    }

    public static class RoleExtensions
    {
        public static string ToRoleName(this AppRole role)
            => role.ToString();
    }
}

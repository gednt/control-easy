namespace ControlEasyReborn.Modules.AccessControl.Application.Permissions;

public static class AccessControlPermissions
{
    public const string Issue = "Access.Control.Issue";
    public const string Replace = "Access.Control.Replace";
    public const string Revoke = "Access.Control.Revoke";
    public const string Operate = "Access.Access.Operate";
    public const string Read = "Access.Read";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Issue,
        Replace,
        Revoke,
        Operate,
        Read
    };
}

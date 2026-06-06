namespace WorshipFlow.Domain.Constants;

public static class SystemPermissions
{
    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string UsersDelete = "Users.Delete";
    public const string UsersManageRoles = "Users.ManageRoles";
    public const string UsersManagePermissions = "Users.ManagePermissions";
    public const string ProfileEditOwn = "Profile.EditOwn";

    public static readonly string[] All =
    [
        UsersView,
        UsersCreate,
        UsersEdit,
        UsersDelete,
        UsersManageRoles,
        UsersManagePermissions,
        ProfileEditOwn
    ];
}

using Microsoft.AspNetCore.Authorization;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Domain.Constants;

namespace WorshipFlow.Api.Authorization;

public static class PermissionPolicies
{
    public const string UsersView = "Permission:" + SystemPermissions.UsersView;
    public const string UsersCreate = "Permission:" + SystemPermissions.UsersCreate;
    public const string UsersEdit = "Permission:" + SystemPermissions.UsersEdit;
    public const string UsersDelete = "Permission:" + SystemPermissions.UsersDelete;
    public const string UsersManageRoles = "Permission:" + SystemPermissions.UsersManageRoles;
    public const string UsersManagePermissions = "Permission:" + SystemPermissions.UsersManagePermissions;
    public const string ProfileEditOwn = "Permission:" + SystemPermissions.ProfileEditOwn;

    public static string PolicyName(string permission) => $"Permission:{permission}";
}

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PermissionAuthorizationHandler(ICurrentUser currentUser, IUserAuthorizationService authorizationService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return;
        }

        if (await authorizationService.HasPermissionAsync(currentUser.TenantId, userId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}

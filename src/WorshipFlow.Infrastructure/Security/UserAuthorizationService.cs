using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Infrastructure.Persistence;

namespace WorshipFlow.Infrastructure.Security;

public sealed class UserAuthorizationService(WorshipFlowDbContext db) : IUserAuthorizationService
{
    public async Task<bool> HasPermissionAsync(Guid tenantId, Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var hasDirectPermission = await db.UserPermissions.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.UserId == userId &&
            x.Permission != null &&
            x.Permission.Name == permission,
            cancellationToken);

        if (hasDirectPermission)
        {
            return true;
        }

        return await db.UserRoles.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.UserId == userId &&
            x.Role != null &&
            x.Role.Permissions.Any(rp => rp.Permission != null && rp.Permission.Name == permission),
            cancellationToken);
    }
}

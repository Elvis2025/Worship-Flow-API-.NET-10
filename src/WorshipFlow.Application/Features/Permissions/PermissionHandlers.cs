using MediatR;
using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Application.Common;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Application.Features.Permissions;

public sealed record GetPermissionsQuery() : IRequest<ApiResponse<IReadOnlyList<PermissionDto>>>;
public sealed record UpdateUserPermissionsCommand(Guid UserId, UpdateUserPermissionsDto Dto) : IRequest<ApiResponse<IReadOnlyList<string>>>;

public sealed class PermissionHandler(IWorshipFlowDbContext db, ICurrentUser currentUser, IAuditService audit)
    : IRequestHandler<GetPermissionsQuery, ApiResponse<IReadOnlyList<PermissionDto>>>, IRequestHandler<UpdateUserPermissionsCommand, ApiResponse<IReadOnlyList<string>>>
{
    public async Task<ApiResponse<IReadOnlyList<PermissionDto>>> Handle(GetPermissionsQuery request, CancellationToken ct)
    {
        var permissions = await db.Permissions.Where(x => !x.IsDeleted).OrderBy(x => x.Name).Select(x => new PermissionDto(x.Id, x.Name, x.Description, x.Module)).ToListAsync(ct);
        return ApiResponse<IReadOnlyList<PermissionDto>>.Ok(permissions);
    }

    public async Task<ApiResponse<IReadOnlyList<string>>> Handle(UpdateUserPermissionsCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.Id == request.UserId && !x.IsDeleted, ct);
        if (user is null) return ApiResponse<IReadOnlyList<string>>.Fail("User not found.");
        var permissions = await db.Permissions.Where(x => request.Dto.Permissions.Contains(x.Name)).ToListAsync(ct);
        db.UserPermissions.RemoveRange(db.UserPermissions.Where(x => x.TenantId == currentUser.TenantId && x.UserId == request.UserId));
        foreach (var permission in permissions) db.UserPermissions.Add(new UserPermission { TenantId = currentUser.TenantId, UserId = request.UserId, PermissionId = permission.Id });
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync("UserPermissionsChanged", nameof(AppUser), request.UserId, new { Permissions = permissions.Select(x => x.Name) }, ct);
        return ApiResponse<IReadOnlyList<string>>.Ok(permissions.Select(x => x.Name).Order().ToArray());
    }
}

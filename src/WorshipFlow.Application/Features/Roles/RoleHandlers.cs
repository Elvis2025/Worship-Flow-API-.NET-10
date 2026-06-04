using MediatR;
using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Application.Common;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Application.Features.Roles;

public sealed record GetRolesQuery() : IRequest<ApiResponse<IReadOnlyList<RoleDto>>>;
public sealed record UpdateUserRolesCommand(Guid UserId, UpdateUserRolesDto Dto) : IRequest<ApiResponse<IReadOnlyList<string>>>;

public sealed class RoleHandler(IWorshipFlowDbContext db, ICurrentUser currentUser, IAuditService audit)
    : IRequestHandler<GetRolesQuery, ApiResponse<IReadOnlyList<RoleDto>>>, IRequestHandler<UpdateUserRolesCommand, ApiResponse<IReadOnlyList<string>>>
{
    public async Task<ApiResponse<IReadOnlyList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        var roles = await db.Roles.Where(x => x.TenantId == currentUser.TenantId && !x.IsDeleted).Include(x => x.Permissions).ThenInclude(x => x.Permission).OrderBy(x => x.Name).Select(x => new RoleDto(x.Id, x.Name, x.Description, x.Permissions.Select(p => p.Permission!.Name).Order().ToArray())).ToListAsync(ct);
        return ApiResponse<IReadOnlyList<RoleDto>>.Ok(roles);
    }

    public async Task<ApiResponse<IReadOnlyList<string>>> Handle(UpdateUserRolesCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.Id == request.UserId && !x.IsDeleted, ct);
        if (user is null) return ApiResponse<IReadOnlyList<string>>.Fail("User not found.");
        var roles = await db.Roles.Where(x => x.TenantId == currentUser.TenantId && request.Dto.Roles.Contains(x.Name)).ToListAsync(ct);
        db.UserRoles.RemoveRange(db.UserRoles.Where(x => x.TenantId == currentUser.TenantId && x.UserId == request.UserId));
        foreach (var role in roles) db.UserRoles.Add(new UserRole { TenantId = currentUser.TenantId, UserId = request.UserId, RoleId = role.Id });
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync("UserRolesChanged", nameof(AppUser), request.UserId, new { Roles = roles.Select(x => x.Name) }, ct);
        return ApiResponse<IReadOnlyList<string>>.Ok(roles.Select(x => x.Name).Order().ToArray());
    }
}

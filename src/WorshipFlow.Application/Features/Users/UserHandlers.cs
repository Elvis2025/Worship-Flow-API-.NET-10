using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Application.Common;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Application.Features.Users;

public sealed record GetUsersQuery(UserFilterDto Filter) : IRequest<ApiResponse<PagedResult<UserListItemDto>>>;
public sealed record GetUserByIdQuery(Guid Id) : IRequest<ApiResponse<UserDto>>;
public sealed record CreateUserCommand(CreateUserDto Dto) : IRequest<ApiResponse<UserDto>>;
public sealed record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : IRequest<ApiResponse<UserDto>>;
public sealed record UpdateUserStatusCommand(Guid Id, UpdateStatusDto Dto) : IRequest<ApiResponse<UserDto>>;
public sealed record DeleteUserCommand(Guid Id) : IRequest<ApiResponse<bool>>;
public sealed record UploadUserPhotoCommand(Guid Id, IFormFile File) : IRequest<ApiResponse<UserDto>>;

public sealed class UserHandler(IWorshipFlowDbContext db, ICurrentUser currentUser, IPasswordHasher hasher, IFileStorage files, IAuditService audit)
    : IRequestHandler<GetUsersQuery, ApiResponse<PagedResult<UserListItemDto>>>, IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>, IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>, IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>, IRequestHandler<UpdateUserStatusCommand, ApiResponse<UserDto>>, IRequestHandler<DeleteUserCommand, ApiResponse<bool>>, IRequestHandler<UploadUserPhotoCommand, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<PagedResult<UserListItemDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var f = request.Filter;
        var query = IncludeUser(db.Users).Where(x => x.TenantId == currentUser.TenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(f.Name)) query = query.Where(x => x.FullName.Contains(f.Name));
        if (!string.IsNullOrWhiteSpace(f.Email)) query = query.Where(x => x.Email.Contains(f.Email.ToLower()));
        if (f.Status.HasValue) query = query.Where(x => x.Status == f.Status.Value);
        if (!string.IsNullOrWhiteSpace(f.Role)) query = query.Where(x => x.Roles.Any(r => r.Role!.Name == f.Role));
        if (!string.IsNullOrWhiteSpace(f.Instrument)) query = query.Where(x => x.MainInstrument.Contains(f.Instrument));
        var total = await query.CountAsync(ct);
        var users = await query.OrderBy(x => x.FullName).Skip((Math.Max(1, f.Page) - 1) * Math.Clamp(f.PageSize, 1, 100)).Take(Math.Clamp(f.PageSize, 1, 100)).ToListAsync(ct);
        var items = users.Select(x => x.ToListItemDto()).ToArray();
        return ApiResponse<PagedResult<UserListItemDto>>.Ok(new PagedResult<UserListItemDto>(items, Math.Max(1, f.Page), Math.Clamp(f.PageSize, 1, 100), total));
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct) => await Find(request.Id, ct) is { } user ? ApiResponse<UserDto>.Ok(user.ToDto()) : ApiResponse<UserDto>.Fail("User not found.");

    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var email = request.Dto.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.TenantId == currentUser.TenantId && x.Email == email && !x.IsDeleted, ct)) return ApiResponse<UserDto>.Fail("Email already exists.");
        var user = new AppUser { TenantId = currentUser.TenantId, FirstName = request.Dto.FirstName.Trim(), LastName = request.Dto.LastName.Trim(), Email = email, Phone = request.Dto.Phone, PasswordHash = hasher.Hash(request.Dto.Password), MainInstrument = request.Dto.MainInstrument.Trim(), VocalRange = request.Dto.VocalRange, ComfortableKey = request.Dto.ComfortableKey, Status = request.Dto.Status, JoinedAt = DateTimeOffset.UtcNow };
        user.FullName = $"{user.FirstName} {user.LastName}".Trim();
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync("UserCreated", nameof(AppUser), user.Id, new { user.Email }, ct);
        return ApiResponse<UserDto>.Ok(user.ToDto(), "User created.");
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await Find(request.Id, ct); if (user is null) return ApiResponse<UserDto>.Fail("User not found.");
        user.FirstName = request.Dto.FirstName.Trim(); user.LastName = request.Dto.LastName.Trim(); user.FullName = $"{user.FirstName} {user.LastName}".Trim(); user.Email = request.Dto.Email.Trim().ToLowerInvariant(); user.Phone = request.Dto.Phone; user.MainInstrument = request.Dto.MainInstrument.Trim(); user.VocalRange = request.Dto.VocalRange; user.ComfortableKey = request.Dto.ComfortableKey; user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct); await audit.RecordAsync("UserUpdated", nameof(AppUser), user.Id, new { user.Email }, ct);
        return ApiResponse<UserDto>.Ok(user.ToDto(), "User updated.");
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserStatusCommand request, CancellationToken ct)
    {
        var user = await Find(request.Id, ct); if (user is null) return ApiResponse<UserDto>.Fail("User not found.");
        user.Status = request.Dto.Status; user.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); await audit.RecordAsync("UserStatusChanged", nameof(AppUser), user.Id, new { user.Status }, ct); return ApiResponse<UserDto>.Ok(user.ToDto());
    }

    public async Task<ApiResponse<bool>> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await Find(request.Id, ct); if (user is null) return ApiResponse<bool>.Fail("User not found.");
        user.IsDeleted = true; user.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); await audit.RecordAsync("UserDeleted", nameof(AppUser), user.Id, null, ct); return ApiResponse<bool>.Ok(true, "User deleted.");
    }

    public async Task<ApiResponse<UserDto>> Handle(UploadUserPhotoCommand request, CancellationToken ct)
    {
        var user = await Find(request.Id, ct); if (user is null) return ApiResponse<UserDto>.Fail("User not found.");
        var ext = Path.GetExtension(request.File.FileName).TrimStart('.').ToLowerInvariant();
        if (!new[] { "jpg", "jpeg", "png", "webp" }.Contains(ext)) return ApiResponse<UserDto>.Fail("Profile photo must be jpg, jpeg, png, or webp.");
        var stored = await files.SaveAsync(request.File, currentUser.TenantId, "profile-photos", ct);
        user.ProfilePhotoUrl = stored.Url; user.UpdatedAt = DateTimeOffset.UtcNow;
        db.FileResources.Add(new FileResource { TenantId = currentUser.TenantId, OwnerUserId = user.Id, OriginalFileName = stored.OriginalFileName, StoredFileName = stored.StoredFileName, ContentType = stored.ContentType, SizeBytes = stored.SizeBytes, Url = stored.Url });
        await db.SaveChangesAsync(ct); await audit.RecordAsync("UserPhotoChanged", nameof(AppUser), user.Id, new { stored.Url }, ct); return ApiResponse<UserDto>.Ok(user.ToDto());
    }

    private Task<AppUser?> Find(Guid id, CancellationToken ct) => IncludeUser(db.Users).FirstOrDefaultAsync(x => x.TenantId == currentUser.TenantId && x.Id == id && !x.IsDeleted, ct);
    private static IQueryable<AppUser> IncludeUser(IQueryable<AppUser> users) => users.Include(x => x.Roles).ThenInclude(x => x.Role)!.ThenInclude(x => x.Permissions).ThenInclude(x => x.Permission).Include(x => x.Permissions).ThenInclude(x => x.Permission);
}

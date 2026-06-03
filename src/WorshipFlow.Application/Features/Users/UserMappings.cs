using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Application.Features.Users;

public static class UserMappings
{
    public static UserDto ToDto(this AppUser user)
    {
        var roles = user.Roles.Select(x => x.Role?.Name).Where(x => x is not null).Cast<string>().Order().ToArray();
        var direct = user.Permissions.Select(x => x.Permission?.Name).Where(x => x is not null).Cast<string>();
        var fromRoles = user.Roles.SelectMany(x => x.Role?.Permissions ?? []).Select(x => x.Permission?.Name).Where(x => x is not null).Cast<string>();
        return new UserDto(user.Id, user.TenantId, user.FullName, user.FirstName, user.LastName, user.Email, user.Phone,
            user.ProfilePhotoUrl, user.MainInstrument, user.VocalRange, user.ComfortableKey, user.Status, user.JoinedAt,
            user.LastLoginAt, roles, direct.Concat(fromRoles).Distinct().Order().ToArray());
    }

    public static UserListItemDto ToListItemDto(this AppUser user) => new(user.Id, user.FullName, user.Email, user.MainInstrument,
        user.Status, user.ProfilePhotoUrl, user.Roles.Select(x => x.Role?.Name).Where(x => x is not null).Cast<string>().Order().ToArray());
}

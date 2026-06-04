namespace WorshipFlow.Application.Features.Roles;
public sealed record RoleDto(Guid Id, string Name, string Description, IReadOnlyList<string> Permissions);
public sealed record UpdateUserRolesDto(IReadOnlyList<string> Roles);

namespace WorshipFlow.Application.Features.Permissions;
public sealed record PermissionDto(Guid Id, string Name, string Description, string Module);
public sealed record UpdateUserPermissionsDto(IReadOnlyList<string> Permissions);

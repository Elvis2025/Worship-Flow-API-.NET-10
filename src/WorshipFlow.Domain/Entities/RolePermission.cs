using WorshipFlow.Domain.Abstractions;

namespace WorshipFlow.Domain.Entities;

public sealed class Role : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}

public sealed class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = "Users";
}

public sealed class UserRole : ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}

public sealed class UserPermission : ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public Guid PermissionId { get; set; }
    public Permission? Permission { get; set; }
}

public sealed class RolePermission : ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public Guid PermissionId { get; set; }
    public Permission? Permission { get; set; }
}

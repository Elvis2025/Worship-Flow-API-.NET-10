using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Domain.Constants;
using WorshipFlow.Domain.Entities;
using WorshipFlow.Domain.Enums;

namespace WorshipFlow.Infrastructure.Persistence;

public sealed class WorshipFlowDbContext(DbContextOptions<WorshipFlowDbContext> options) : DbContext(options), IWorshipFlowDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<UserAvailability> UserAvailability => Set<UserAvailability>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FileResource> FileResources => Set<FileResource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b => { b.HasIndex(x => x.Slug).IsUnique(); b.Property(x => x.Name).HasMaxLength(160).IsRequired(); });
        modelBuilder.Entity<AppUser>(b =>
        {
            b.ToTable("Users"); b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique(); b.Property(x => x.Email).HasMaxLength(320).IsRequired(); b.Property(x => x.FullName).HasMaxLength(240).IsRequired(); b.Property(x => x.FirstName).HasMaxLength(120).IsRequired(); b.Property(x => x.LastName).HasMaxLength(120).IsRequired(); b.Property(x => x.MainInstrument).HasMaxLength(120).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40); b.HasQueryFilter(x => !x.IsDeleted);
        });
        modelBuilder.Entity<Role>(b => { b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique(); b.Property(x => x.Name).HasMaxLength(120).IsRequired(); });
        modelBuilder.Entity<Permission>(b => { b.HasIndex(x => x.Name).IsUnique(); b.Property(x => x.Name).HasMaxLength(120).IsRequired(); });
        modelBuilder.Entity<UserRole>(b => { b.HasKey(x => new { x.UserId, x.RoleId }); b.HasOne(x => x.User).WithMany(x => x.Roles).HasForeignKey(x => x.UserId); b.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId); });
        modelBuilder.Entity<UserPermission>(b => { b.HasKey(x => new { x.UserId, x.PermissionId }); b.HasOne(x => x.User).WithMany(x => x.Permissions).HasForeignKey(x => x.UserId); });
        modelBuilder.Entity<RolePermission>(b => { b.HasKey(x => new { x.RoleId, x.PermissionId }); b.HasOne(x => x.Role).WithMany(x => x.Permissions).HasForeignKey(x => x.RoleId); });
        modelBuilder.Entity<RefreshToken>(b => { b.HasIndex(x => x.TokenHash).IsUnique(); b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired(); });
        modelBuilder.Entity<DeviceToken>(b => { b.HasIndex(x => new { x.TenantId, x.UserId, x.Token }).IsUnique(); });
        modelBuilder.Entity<UserAvailability>(b => { b.HasIndex(x => new { x.TenantId, x.UserId, x.DayOfWeek }).IsUnique(); b.Property(x => x.DayOfWeek).HasConversion<string>(); });
        modelBuilder.Entity<AuditLog>(b => { b.Property(x => x.Action).HasMaxLength(120).IsRequired(); b.Property(x => x.EntityName).HasMaxLength(120).IsRequired(); });
        modelBuilder.Entity<FileResource>(b => { b.Property(x => x.Url).HasMaxLength(1024).IsRequired(); });
    }

    public static async Task SeedAsync(WorshipFlowDbContext db, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(ct) ?? new Tenant { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Default Church", Slug = "default" };
        if (db.Entry(tenant).State == EntityState.Detached) db.Tenants.Add(tenant);
        foreach (var permissionName in SystemPermissions.All)
            if (!await db.Permissions.AnyAsync(x => x.Name == permissionName, ct)) db.Permissions.Add(new Permission { Name = permissionName, Description = permissionName, Module = "M1.Users" });
        await db.SaveChangesAsync(ct);
        var permissions = await db.Permissions.ToListAsync(ct);
        foreach (var roleName in SystemRoles.All)
        {
            var role = await db.Roles.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Name == roleName, ct);
            if (role is null) { role = new Role { TenantId = tenant.Id, Name = roleName, Description = roleName }; db.Roles.Add(role); await db.SaveChangesAsync(ct); }
            if (roleName == SystemRoles.Administrator)
                foreach (var p in permissions.Where(p => role.Permissions.All(rp => rp.PermissionId != p.Id))) db.RolePermissions.Add(new RolePermission { TenantId = tenant.Id, RoleId = role.Id, PermissionId = p.Id });
        }
        await db.SaveChangesAsync(ct);

        var administratorRole = await db.Roles.FirstAsync(x => x.TenantId == tenant.Id && x.Name == SystemRoles.Administrator, ct);
        var administratorEmail = "admin@worshipflow.local";
        var administrator = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Email == administratorEmail, ct);
        if (administrator is null)
        {
            administrator = new AppUser
            {
                TenantId = tenant.Id,
                FirstName = "System",
                LastName = "Administrator",
                FullName = "System Administrator",
                Email = administratorEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
                MainInstrument = "Administration",
                Status = UserStatus.Active,
                JoinedAt = DateTimeOffset.UtcNow
            };
            db.Users.Add(administrator);
            await db.SaveChangesAsync(ct);
        }

        if (!await db.UserRoles.AnyAsync(x => x.TenantId == tenant.Id && x.UserId == administrator.Id && x.RoleId == administratorRole.Id, ct))
        {
            db.UserRoles.Add(new UserRole { TenantId = tenant.Id, UserId = administrator.Id, RoleId = administratorRole.Id });
            await db.SaveChangesAsync(ct);
        }
    }
}

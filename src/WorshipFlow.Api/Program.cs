using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WorshipFlow.Api.Authorization;
using WorshipFlow.Domain.Constants;
using WorshipFlow.Application.DependencyInjection;
using WorshipFlow.Infrastructure.DependencyInjection;
using WorshipFlow.Infrastructure.Persistence;
using WorshipFlow.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddApplicationServices();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PermissionPolicies.UsersView, policy => policy.Requirements.Add(new PermissionRequirement(SystemPermissions.UsersView)))
    .AddPolicy(PermissionPolicies.UsersCreate, policy => policy.Requirements.Add(new PermissionRequirement(SystemPermissions.UsersCreate)))
    .AddPolicy(PermissionPolicies.UsersEdit, policy => policy.Requirements.Add(new PermissionRequirement(SystemPermissions.UsersEdit)))
    .AddPolicy(PermissionPolicies.UsersDelete, policy => policy.Requirements.Add(new PermissionRequirement(SystemPermissions.UsersDelete)))
    .AddPolicy(PermissionPolicies.UsersManageRoles, policy => policy.Requirements.Add(new PermissionRequirement(SystemPermissions.UsersManageRoles)))
    .AddPolicy(PermissionPolicies.UsersManagePermissions, policy => policy.Requirements.Add(new PermissionRequirement(SystemPermissions.UsersManagePermissions)))
    .AddPolicy(PermissionPolicies.ProfileEditOwn, policy => policy.Requirements.Add(new PermissionRequirement(SystemPermissions.ProfileEditOwn)));
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("api", limiter => { limiter.PermitLimit = 120; limiter.Window = TimeSpan.FromMinutes(1); }));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorshipFlowDbContext>();
    await db.Database.EnsureCreatedAsync();
    await WorshipFlowDbContext.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers().RequireRateLimiting("api");
app.Run();

public partial class Program { }

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Infrastructure.Auditing;
using WorshipFlow.Infrastructure.Files;
using WorshipFlow.Infrastructure.Persistence;
using WorshipFlow.Infrastructure.Security;

namespace WorshipFlow.Infrastructure.DependencyInjection;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) || provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            services.AddDbContext<WorshipFlowDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
        else
            services.AddDbContext<WorshipFlowDbContext>(o => o.UseSqlite(configuration.GetConnectionString("Sqlite") ?? "Data Source=worshipflow.db"));

        services.AddScoped<IWorshipFlowDbContext>(sp => sp.GetRequiredService<WorshipFlowDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(WorshipFlow.Infrastructure.Repositories.EfRepository<>));
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddHttpContextAccessor();

        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "dev-key-change-me-dev-key-change-me-32");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
        services.AddAuthorization();
        return services;
    }
}

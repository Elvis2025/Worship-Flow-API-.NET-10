using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace WorshipFlow.Application.DependencyInjection;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}

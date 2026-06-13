using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Handlers;
using ControlEasyReborn.Modules.Tenants.Application.Validators;
using ControlEasyReborn.Modules.Tenants.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlEasyReborn.Modules.Tenants.Infrastructure.DI;

public static class TenantsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTenantsModule(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantContext, HttpTenantContext>();
        services.AddSingleton<TenantAwareLinqFactory>();
        services.AddSingleton<ITenantAwareLinqFactory>(sp => sp.GetRequiredService<TenantAwareLinqFactory>());

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<CreateTenantHandler>();
        services.AddScoped<GetTenantHandler>();
        services.AddScoped<SuspendTenantHandler>();
        services.AddScoped<ResumeTenantHandler>();
        services.AddScoped<IValidator<ControlEasyReborn.Modules.Tenants.Application.Contracts.CreateTenantRequest>, CreateTenantRequestValidator>();
        return services;
    }
}

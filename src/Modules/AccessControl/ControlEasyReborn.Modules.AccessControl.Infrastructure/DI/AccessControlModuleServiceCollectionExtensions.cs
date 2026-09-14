using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlEasyReborn.Modules.AccessControl.Infrastructure.DI;

public static class AccessControlModuleServiceCollectionExtensions
{
    public static IServiceCollection AddAccessControlModule(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantContext, HttpTenantContext>();
        services.AddSingleton<TenantAwareLinqFactory>();
        services.AddSingleton<ITenantAwareLinqFactory>(sp => sp.GetRequiredService<TenantAwareLinqFactory>());
        return services;
    }
}

using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Infrastructure.Persistence;
using ControlEasyReborn.Modules.AccessControl.Infrastructure.Services;
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

        services.AddSingleton<IOpaqueTokenIssuer, OpaqueTokenIssuer>();

        services.AddScoped<IAccessCredentialRepository, AccessCredentialRepository>();
        services.AddScoped<IAccessEventRepository, AccessEventRepository>();
        services.AddScoped<IRefusedScanAttemptRepository, RefusedScanAttemptRepository>();
        services.AddScoped<IAccessLookupAuditRepository, AccessLookupAuditRepository>();
        services.AddScoped<ICredentialLifecycleActionRepository, CredentialLifecycleActionRepository>();

        return services;
    }
}

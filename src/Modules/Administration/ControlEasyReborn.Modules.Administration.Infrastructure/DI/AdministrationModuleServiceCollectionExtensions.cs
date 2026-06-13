using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Handlers;
using ControlEasyReborn.Modules.Administration.Application.Validators;
using ControlEasyReborn.Modules.Administration.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlEasyReborn.Modules.Administration.Infrastructure.DI;

public static class AdministrationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddAdministrationModule(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantContext, HttpTenantContext>();
        services.AddSingleton<TenantAwareLinqFactory>();
        services.AddSingleton<ITenantAwareLinqFactory>(sp => sp.GetRequiredService<TenantAwareLinqFactory>());

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<CreateAuditLogHandler>();
        services.AddScoped<ListAuditLogHandler>();
        services.AddScoped<GetEntityAuditLogHandler>();
        services.AddScoped<CreateConfigurationHandler>();
        services.AddScoped<GetConfigurationHandler>();
        services.AddScoped<GetConfigurationByKeyHandler>();
        services.AddScoped<ListConfigurationsHandler>();
        services.AddScoped<UpdateConfigurationHandler>();
        services.AddScoped<IValidator<CreateAuditLogRequest>, CreateAuditLogRequestValidator>();
        services.AddScoped<IValidator<CreateConfigurationRequest>, CreateConfigurationRequestValidator>();
        services.AddScoped<IValidator<UpdateConfigurationRequest>, UpdateConfigurationRequestValidator>();
        return services;
    }
}
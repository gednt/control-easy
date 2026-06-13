using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Application.Handlers;
using ControlEasyReborn.Modules.Residents.Application.Validators;
using ControlEasyReborn.Modules.Residents.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlEasyReborn.Modules.Residents.Infrastructure.DI;

public static class ResidentsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddResidentsModule(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantContext, HttpTenantContext>();
        services.AddSingleton<TenantAwareLinqFactory>();
        services.AddSingleton<ITenantAwareLinqFactory>(sp => sp.GetRequiredService<TenantAwareLinqFactory>());

        services.AddScoped<IResidentRepository, ResidentRepository>();
        services.AddScoped<CreateResidentHandler>();
        services.AddScoped<GetResidentHandler>();
        services.AddScoped<ListResidentsHandler>();
        services.AddScoped<UpdateResidentHandler>();
        services.AddScoped<IValidator<CreateResidentRequest>, CreateResidentRequestValidator>();
        services.AddScoped<IValidator<UpdateResidentRequest>, UpdateResidentRequestValidator>();
        return services;
    }
}
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Apartments.Application.Handlers;
using ControlEasyReborn.Modules.Apartments.Application.Validators;
using ControlEasyReborn.Modules.Apartments.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlEasyReborn.Modules.Apartments.Infrastructure.DI;

public static class ApartmentsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddApartmentsModule(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantContext, HttpTenantContext>();
        services.AddSingleton<TenantAwareLinqFactory>();
        services.AddSingleton<ITenantAwareLinqFactory>(sp => sp.GetRequiredService<TenantAwareLinqFactory>());

        services.AddScoped<IApartmentRepository, ApartmentRepository>();
        services.AddScoped<CreateApartmentHandler>();
        services.AddScoped<GetApartmentHandler>();
        services.AddScoped<ListApartmentsHandler>();
        services.AddScoped<UpdateApartmentHandler>();
        services.AddScoped<IValidator<CreateApartmentRequest>, CreateApartmentRequestValidator>();
        services.AddScoped<IValidator<UpdateApartmentRequest>, UpdateApartmentRequestValidator>();
        return services;
    }
}

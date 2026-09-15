using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Contracts;
using ControlEasyReborn.Modules.Vehicles.Application.Handlers;
using ControlEasyReborn.Modules.Vehicles.Application.Validators;
using ControlEasyReborn.Modules.Vehicles.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlEasyReborn.Modules.Vehicles.Infrastructure.DI;

public static class VehiclesModuleServiceCollectionExtensions
{
    public static IServiceCollection AddVehiclesModule(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantContext, HttpTenantContext>();
        services.AddSingleton<TenantAwareLinqFactory>();
        services.AddSingleton<ITenantAwareLinqFactory>(sp => sp.GetRequiredService<TenantAwareLinqFactory>());

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleDirectory, VehicleDirectoryRepository>();
        services.AddScoped<CreateVehicleHandler>();
        services.AddScoped<GetVehicleHandler>();
        services.AddScoped<ListVehiclesHandler>();
        services.AddScoped<UpdateVehicleHandler>();
        services.AddScoped<IValidator<CreateVehicleRequest>, CreateVehicleRequestValidator>();
        services.AddScoped<IValidator<UpdateVehicleRequest>, UpdateVehicleRequestValidator>();
        return services;
    }
}
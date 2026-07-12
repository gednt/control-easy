namespace ControlEasyReborn.SharedKernel.FeatureFlags;

public static class FeatureFlags
{
    public const string ResidentsUseWeb = "Residents.UseWeb";
    public const string VisitsUseWeb = "Visits.UseWeb";
    public const string VehiclesUseWeb = "Vehicles.UseWeb";
    public const string ServiceProvidersUseWeb = "ServiceProviders.UseWeb";
    public const string AdministrationUseWeb = "Administration.UseWeb";

    public static readonly string[] All =
    [
        ResidentsUseWeb,
        VisitsUseWeb,
        VehiclesUseWeb,
        ServiceProvidersUseWeb,
        AdministrationUseWeb
    ];
}
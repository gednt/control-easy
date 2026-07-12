namespace ControlEasyReborn.Modules.Security.Application;

public static class Permissions
{
    public const string VisitsCheckIn = "Visits.CheckIn";
    public const string VisitsCheckOut = "Visits.CheckOut";
    public const string VisitsRead = "Visits.Read";
    public const string ApartmentsRead = "Apartments.Read";
    public const string ApartmentsWrite = "Apartments.Write";
    public const string ResidentsRead = "Residents.Read";
    public const string ResidentsWrite = "Residents.Write";
    public const string VehiclesRead = "Vehicles.Read";
    public const string VehiclesWrite = "Vehicles.Write";
    public const string ServiceProvidersRead = "ServiceProviders.Read";
    public const string ServiceProvidersWrite = "ServiceProviders.Write";
    public const string ReportsRead = "Reports.Read";

    public static readonly string[] All =
    [
        VisitsCheckIn, VisitsCheckOut, VisitsRead,
        ApartmentsRead, ApartmentsWrite,
        ResidentsRead, ResidentsWrite,
        VehiclesRead, VehiclesWrite,
        ServiceProvidersRead, ServiceProvidersWrite,
        ReportsRead
    ];
}
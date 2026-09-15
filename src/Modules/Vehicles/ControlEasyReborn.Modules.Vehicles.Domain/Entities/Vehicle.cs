namespace ControlEasyReborn.Modules.Vehicles.Domain.Entities;

public enum VehicleType
{
    Car = 0,
    Motorcycle = 1,
    Truck = 2,
    Other = 3
}

public sealed class Vehicle
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Plate { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public string? Color { get; private set; }
    public Guid? ApartmentId { get; private set; }
    public Guid? OwnerResidentId { get; private set; }
    public string? OwnerName { get; private set; }
    public VehicleType VehicleType { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Vehicle() { }

    public Vehicle(Guid id, Guid tenantId, string plate, string? brand, string? model, string? color, Guid? apartmentId, string? ownerName, VehicleType vehicleType, bool active, DateTime createdAtUtc, DateTime? updatedAtUtc = null, Guid? ownerResidentId = null)
    {
        Id = id;
        TenantId = tenantId;
        Plate = plate;
        Brand = brand;
        Model = model;
        Color = color;
        ApartmentId = apartmentId;
        OwnerName = ownerName;
        VehicleType = vehicleType;
        Active = active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        OwnerResidentId = ownerResidentId;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string plate, string? brand, string? model, string? color, Guid? apartmentId, string? ownerName, VehicleType vehicleType, Guid? ownerResidentId = null)
    {
        Plate = plate;
        Brand = brand;
        Model = model;
        Color = color;
        ApartmentId = apartmentId;
        OwnerName = ownerName;
        VehicleType = vehicleType;
        OwnerResidentId = ownerResidentId ?? OwnerResidentId;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
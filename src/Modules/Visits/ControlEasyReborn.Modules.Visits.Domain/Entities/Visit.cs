namespace ControlEasyReborn.Modules.Visits.Domain.Entities;

public enum VisitStatus
{
    Pending = 0,
    CheckedIn = 1,
    CheckedOut = 2,
    Cancelled = 3
}

public sealed class Visit
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string VisitorName { get; private set; } = string.Empty;
    public string VisitorDocument { get; private set; } = string.Empty;
    public string? VisitorPhone { get; private set; }
    public Guid ApartmentId { get; private set; }
    public string DestinationBlock { get; private set; } = string.Empty;
    public string DestinationUnit { get; private set; } = string.Empty;
    public string? Purpose { get; private set; }
    public VisitStatus Status { get; private set; }
    public Guid? AttendantProfileId { get; private set; }
    public Guid? GatehouseId { get; private set; }
    public DateTime? CheckedInAtUtc { get; private set; }
    public DateTime? CheckedOutAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Visit() { }

    public Visit(Guid id, Guid tenantId, string visitorName, string visitorDocument, string? visitorPhone, Guid apartmentId, string destinationBlock, string destinationUnit, string? purpose, VisitStatus status, Guid? attendantProfileId, Guid? gatehouseId, DateTime? checkedInAtUtc, DateTime? checkedOutAtUtc, DateTime createdAtUtc, DateTime? updatedAtUtc = null)
    {
        if (apartmentId == Guid.Empty) throw new InvalidOperationException("Apartment id is required.");
        if (string.IsNullOrWhiteSpace(destinationBlock)) throw new InvalidOperationException("Destination block snapshot is required.");
        if (string.IsNullOrWhiteSpace(destinationUnit)) throw new InvalidOperationException("Destination unit snapshot is required.");

        Id = id;
        TenantId = tenantId;
        VisitorName = visitorName;
        VisitorDocument = visitorDocument;
        VisitorPhone = visitorPhone;
        ApartmentId = apartmentId;
        DestinationBlock = destinationBlock;
        DestinationUnit = destinationUnit;
        Purpose = purpose;
        Status = status;
        AttendantProfileId = attendantProfileId;
        GatehouseId = gatehouseId;
        CheckedInAtUtc = checkedInAtUtc;
        CheckedOutAtUtc = checkedOutAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void CheckIn(Guid attendantProfileId, Guid? gatehouseId)
    {
        if (Status != VisitStatus.Pending)
            throw new InvalidOperationException("Only pending visits can be checked in.");

        Status = VisitStatus.CheckedIn;
        AttendantProfileId = attendantProfileId;
        GatehouseId = gatehouseId;
        CheckedInAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void CheckOut()
    {
        if (Status != VisitStatus.CheckedIn)
            throw new InvalidOperationException("Only checked-in visits can be checked out.");

        Status = VisitStatus.CheckedOut;
        CheckedOutAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == VisitStatus.CheckedOut || Status == VisitStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a checked-out or already cancelled visit.");

        Status = VisitStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
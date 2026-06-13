using ControlEasyReborn.Modules.Security.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Security;

public sealed class AttendantProfileEntityTests
{
    [Fact]
    public void Deactivate_sets_Active_false_and_UpdatedAtUtc()
    {
        var profile = new AttendantProfile(
            id: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            displayName: "Test User",
            shiftId: null,
            gatehouseId: null,
            permissions: "Visits.Read",
            active: true,
            createdAtUtc: DateTime.UtcNow,
            updatedAtUtc: null);

        profile.Deactivate();

        profile.Active.Should().BeFalse();
        profile.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_updates_all_fields_and_UpdatedAtUtc()
    {
        var profile = new AttendantProfile(
            id: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            displayName: "Old Name",
            shiftId: null,
            gatehouseId: null,
            permissions: "Visits.Read",
            active: true,
            createdAtUtc: DateTime.UtcNow,
            updatedAtUtc: null);

        var newShiftId = Guid.NewGuid();
        var newGatehouseId = Guid.NewGuid();
        profile.UpdateDetails("New Name", newShiftId, newGatehouseId, "Visits.Read,Residents.Read");

        profile.DisplayName.Should().Be("New Name");
        profile.ShiftId.Should().Be(newShiftId);
        profile.GatehouseId.Should().Be(newGatehouseId);
        profile.Permissions.Should().Be("Visits.Read,Residents.Read");
        profile.UpdatedAtUtc.Should().NotBeNull();
    }
}
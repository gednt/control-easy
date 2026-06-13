using ControlEasyReborn.Modules.Security.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Security;

public sealed class ShiftEntityTests
{
    [Fact]
    public void CrossesMidnight_is_true_when_StartTime_after_EndTime()
    {
        var shift = new Shift(
            id: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            name: "Night Shift",
            startTime: new TimeSpan(22, 0, 0),
            endTime: new TimeSpan(6, 0, 0));

        shift.CrossesMidnight.Should().BeTrue();
    }

    [Fact]
    public void CrossesMidnight_is_false_when_StartTime_before_EndTime()
    {
        var shift = new Shift(
            id: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            name: "Day Shift",
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(16, 0, 0));

        shift.CrossesMidnight.Should().BeFalse();
    }

    [Fact]
    public void CrossesMidnight_is_false_when_StartTime_equals_EndTime()
    {
        var shift = new Shift(
            id: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            name: "Same Time",
            startTime: new TimeSpan(12, 0, 0),
            endTime: new TimeSpan(12, 0, 0));

        shift.CrossesMidnight.Should().BeFalse();
    }
}
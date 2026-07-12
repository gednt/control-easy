using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Validators;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Security;

public sealed class CreateAttendantProfileRequestValidatorTests
{
    private readonly CreateAttendantProfileRequestValidator _sut = new();

    [Fact]
    public async Task Valid_request_passes_validation()
    {
        var request = new CreateAttendantProfileRequest(
            UserId: Guid.NewGuid(),
            DisplayName: "Test Attendant",
            ShiftId: null,
            GatehouseId: null,
            Permissions: "Visits.Read");

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Empty_UserId_fails_validation()
    {
        var request = new CreateAttendantProfileRequest(
            UserId: Guid.Empty,
            DisplayName: "Test",
            ShiftId: null,
            GatehouseId: null,
            Permissions: "Visits.Read");

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Empty_Permissions_fails_validation()
    {
        var request = new CreateAttendantProfileRequest(
            UserId: Guid.NewGuid(),
            DisplayName: "Test",
            ShiftId: null,
            GatehouseId: null,
            Permissions: "");

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Permissions");
    }

    [Fact]
    public async Task DisplayName_exceeding_200_chars_fails_validation()
    {
        var request = new CreateAttendantProfileRequest(
            UserId: Guid.NewGuid(),
            DisplayName: new string('x', 201),
            ShiftId: null,
            GatehouseId: null,
            Permissions: "Visits.Read");

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }
}
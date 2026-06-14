using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Apartments.Application.Validators;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Apartments;

public sealed class CreateApartmentRequestValidatorTests
{
    private readonly CreateApartmentRequestValidator _sut = new();

    [Fact]
    public async Task Valid_request_passes_validation()
    {
        var result = await _sut.ValidateAsync(new CreateApartmentRequest("A", "101"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Empty_block_fails_validation()
    {
        var result = await _sut.ValidateAsync(new CreateApartmentRequest("", "101"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Block");
    }

    [Fact]
    public async Task Empty_unit_fails_validation()
    {
        var result = await _sut.ValidateAsync(new CreateApartmentRequest("A", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Unit");
    }

    [Fact]
    public async Task Block_exceeding_50_chars_fails_validation()
    {
        var result = await _sut.ValidateAsync(new CreateApartmentRequest(new string('x', 51), "101"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Block");
    }
}

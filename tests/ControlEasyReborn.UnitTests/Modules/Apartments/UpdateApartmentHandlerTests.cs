using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Apartments.Application.Errors;
using ControlEasyReborn.Modules.Apartments.Application.Handlers;
using ControlEasyReborn.Modules.Apartments.Application.Validators;
using ControlEasyReborn.Modules.Apartments.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Apartments;

public sealed class UpdateApartmentHandlerTests
{
    private readonly IApartmentRepository _apartments;
    private readonly IValidator<UpdateApartmentRequest> _validator;
    private readonly UpdateApartmentHandler _sut;

    public UpdateApartmentHandlerTests()
    {
        _apartments = Substitute.For<IApartmentRepository>();
        _validator = new UpdateApartmentRequestValidator();
        _sut = new UpdateApartmentHandler(_apartments, _validator);
    }

    [Fact]
    public async Task HandleAsync_with_valid_request_updates_apartment()
    {
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var apartment = new Apartment(id, tenantId, "A", "101", true, DateTime.UtcNow);
        _apartments.FindAsync(id, Arg.Any<CancellationToken>()).Returns(apartment);
        _apartments.FindByBlockUnitAsync("A", "102", Arg.Any<CancellationToken>()).Returns((Apartment?)null);

        var response = await _sut.HandleAsync(id, new UpdateApartmentRequest("A", "102", Active: true), CancellationToken.None);

        response.Unit.Should().Be("102");
        await _apartments.Received(1).UpdateAsync(apartment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_when_not_found_throws_NotFoundException()
    {
        var id = Guid.NewGuid();
        _apartments.FindAsync(id, Arg.Any<CancellationToken>()).Returns((Apartment?)null);

        var act = () => _sut.HandleAsync(id, new UpdateApartmentRequest("A", "101", Active: true), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_deactivate_sets_active_false()
    {
        var id = Guid.NewGuid();
        var apartment = new Apartment(id, Guid.NewGuid(), "A", "101", true, DateTime.UtcNow);
        _apartments.FindAsync(id, Arg.Any<CancellationToken>()).Returns(apartment);
        _apartments.FindByBlockUnitAsync("A", "101", Arg.Any<CancellationToken>()).Returns(apartment);

        var response = await _sut.HandleAsync(id, new UpdateApartmentRequest("A", "101", Active: false), CancellationToken.None);

        response.Active.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_with_duplicate_block_unit_for_other_id_throws_ConflictException()
    {
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var apartment = new Apartment(id, tenantId, "A", "101", true, DateTime.UtcNow);
        var duplicate = new Apartment(Guid.NewGuid(), tenantId, "B", "202", true, DateTime.UtcNow);
        _apartments.FindAsync(id, Arg.Any<CancellationToken>()).Returns(apartment);
        _apartments.FindByBlockUnitAsync("B", "202", Arg.Any<CancellationToken>()).Returns(duplicate);

        var act = () => _sut.HandleAsync(id, new UpdateApartmentRequest("B", "202", Active: true), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}

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
using ValidationException = ControlEasyReborn.Modules.Apartments.Application.Errors.ValidationException;

namespace ControlEasyReborn.UnitTests.Modules.Apartments;

public sealed class CreateApartmentHandlerTests
{
    private readonly IApartmentRepository _apartments;
    private readonly IValidator<CreateApartmentRequest> _validator;
    private readonly CreateApartmentHandler _sut;

    public CreateApartmentHandlerTests()
    {
        _apartments = Substitute.For<IApartmentRepository>();
        _validator = new CreateApartmentRequestValidator();
        _sut = new CreateApartmentHandler(_apartments, _validator);
    }

    [Fact]
    public async Task HandleAsync_with_valid_request_creates_apartment()
    {
        var tenantId = Guid.NewGuid();
        _apartments.FindByBlockUnitAsync("A", "101", Arg.Any<CancellationToken>()).Returns((Apartment?)null);

        var response = await _sut.HandleAsync(new CreateApartmentRequest("A", "101"), tenantId, CancellationToken.None);

        response.Block.Should().Be("A");
        response.Unit.Should().Be("101");
        response.TenantId.Should().Be(tenantId);
        response.Active.Should().BeTrue();
        await _apartments.Received(1).AddAsync(
            Arg.Is<Apartment>(a => a.Block == "A" && a.Unit == "101" && a.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_with_duplicate_block_unit_throws_ConflictException()
    {
        var tenantId = Guid.NewGuid();
        var existing = new Apartment(Guid.NewGuid(), tenantId, "A", "101", true, DateTime.UtcNow);
        _apartments.FindByBlockUnitAsync("A", "101", Arg.Any<CancellationToken>()).Returns(existing);

        var act = () => _sut.HandleAsync(new CreateApartmentRequest("A", "101"), tenantId, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task HandleAsync_with_empty_block_throws_ValidationException()
    {
        var act = () => _sut.HandleAsync(new CreateApartmentRequest("", "101"), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}

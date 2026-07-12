using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Application.Errors;
using ControlEasyReborn.Modules.Residents.Application.Handlers;
using ControlEasyReborn.Modules.Residents.Application.Validators;
using ControlEasyReborn.Modules.Residents.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Residents;

public sealed class UpdateResidentHandlerTests
{
    private readonly IResidentRepository _residents;
    private readonly IValidator<UpdateResidentRequest> _validator;
    private readonly UpdateResidentHandler _sut;

    public UpdateResidentHandlerTests()
    {
        _residents = Substitute.For<IResidentRepository>();
        _validator = new UpdateResidentRequestValidator();
        _sut = new UpdateResidentHandler(_residents, _validator);
    }

    [Fact]
    public async Task HandleAsync_with_valid_request_updates_resident()
    {
        var id = Guid.NewGuid();
        var resident = new Resident(
            id,
            Guid.NewGuid(),
            "Maria Silva",
            "52998224725",
            "maria@test.com",
            "11999990001",
            null,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        _residents.FindAsync(id, Arg.Any<CancellationToken>()).Returns(resident);

        var response = await _sut.HandleAsync(
            id,
            new UpdateResidentRequest("Maria Silva", "52998224725", "maria@test.com", "11999990001", null, Active: true),
            CancellationToken.None);

        response.Name.Should().Be("Maria Silva");
        response.Active.Should().BeTrue();
        await _residents.Received(1).UpdateAsync(resident, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_deactivate_sets_active_false()
    {
        var id = Guid.NewGuid();
        var resident = new Resident(
            id,
            Guid.NewGuid(),
            "Maria Silva",
            "52998224725",
            "maria@test.com",
            "11999990001",
            null,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        _residents.FindAsync(id, Arg.Any<CancellationToken>()).Returns(resident);

        var response = await _sut.HandleAsync(
            id,
            new UpdateResidentRequest("Maria Silva", "52998224725", "maria@test.com", "11999990001", null, Active: false),
            CancellationToken.None);

        response.Active.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_when_not_found_throws_NotFoundException()
    {
        var id = Guid.NewGuid();
        _residents.FindAsync(id, Arg.Any<CancellationToken>()).Returns((Resident?)null);

        var act = () => _sut.HandleAsync(
            id,
            new UpdateResidentRequest("Maria Silva", "52998224725", null, null, null, Active: true),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Contracts;
using ControlEasyReborn.Modules.Photos.Application.Errors;
using ControlEasyReborn.Modules.Photos.Application.Handlers;
using ControlEasyReborn.Modules.Photos.Application.Validators;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Photos;

public sealed class UpdateConsentPolicyHandlerTests
{
    private readonly ITenantConsentPolicyRepository _policyRepo = Substitute.For<ITenantConsentPolicyRepository>();
    private readonly UpdateConsentPolicyRequestValidator _validator = new();
    private readonly UpdateConsentPolicyHandler _sut;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _profileId = Guid.NewGuid();

    public UpdateConsentPolicyHandlerTests()
    {
        _sut = new UpdateConsentPolicyHandler(_policyRepo, _validator);
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDoesNotExist_CreatesNewPolicyAndReturnsResponse()
    {
        _policyRepo.FindByCategoryAsync(_tenantId, SubjectCategories.Visitor, Arg.Any<CancellationToken>())
            .Returns((TenantConsentPolicy?)null);

        var request = new UpdateConsentPolicyRequest(SubjectCategories.Visitor, PhotoRequired: true, DwellTimeLimitMinutes: 120);

        var response = await _sut.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.Should().NotBeNull();
        response.SubjectCategory.Should().Be(SubjectCategories.Visitor);
        response.PhotoRequired.Should().BeTrue();
        response.DwellTimeLimitMinutes.Should().Be(120);
        response.UpdatedByProfileId.Should().Be(_profileId);

        await _policyRepo.Received(1).UpsertAsync(Arg.Is<TenantConsentPolicy>(p =>
            p.TenantId == _tenantId &&
            p.SubjectCategory == SubjectCategories.Visitor &&
            p.PhotoRequired &&
            p.DwellTimeLimitMinutes == 120), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyExists_UpdatesExistingPolicy()
    {
        var existing = new TenantConsentPolicy(
            id: Guid.NewGuid(),
            tenantId: _tenantId,
            subjectCategory: SubjectCategories.Visitor,
            photoRequired: false,
            dwellTimeLimitMinutes: 60,
            updatedByProfileId: null,
            createdAtUtc: DateTime.UtcNow.AddDays(-1));

        _policyRepo.FindByCategoryAsync(_tenantId, SubjectCategories.Visitor, Arg.Any<CancellationToken>())
            .Returns(existing);

        var request = new UpdateConsentPolicyRequest(SubjectCategories.Visitor, PhotoRequired: true, DwellTimeLimitMinutes: 180);

        var response = await _sut.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.Should().NotBeNull();
        response.PhotoRequired.Should().BeTrue();
        response.DwellTimeLimitMinutes.Should().Be(180);
        response.UpdatedByProfileId.Should().Be(_profileId);
        response.UpdatedAtUtc.Should().NotBeNull();

        await _policyRepo.Received(1).UpsertAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCategory_ThrowsValidationException()
    {
        var request = new UpdateConsentPolicyRequest("invalid_category", PhotoRequired: true, DwellTimeLimitMinutes: 60);

        var act = () => _sut.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("SubjectCategory"));
    }

    [Fact]
    public async Task HandleAsync_WithZeroOrNegativeDwellTime_ThrowsValidationException()
    {
        var request = new UpdateConsentPolicyRequest(SubjectCategories.Visitor, PhotoRequired: true, DwellTimeLimitMinutes: 0);

        var act = () => _sut.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("DwellTimeLimitMinutes"));
    }

    [Fact]
    public async Task HandleGetAsync_WhenFound_ReturnsResponse()
    {
        var policy = new TenantConsentPolicy(
            id: Guid.NewGuid(),
            tenantId: _tenantId,
            subjectCategory: SubjectCategories.Dweller,
            photoRequired: false,
            dwellTimeLimitMinutes: null,
            updatedByProfileId: null,
            createdAtUtc: DateTime.UtcNow);

        _policyRepo.FindByCategoryAsync(_tenantId, SubjectCategories.Dweller, Arg.Any<CancellationToken>())
            .Returns(policy);

        var result = await _sut.HandleGetAsync(_tenantId, SubjectCategories.Dweller, CancellationToken.None);

        result.Should().NotBeNull();
        result!.SubjectCategory.Should().Be(SubjectCategories.Dweller);
    }

    [Fact]
    public async Task HandleGetAsync_WhenNotFound_ReturnsNull()
    {
        _policyRepo.FindByCategoryAsync(_tenantId, SubjectCategories.Dweller, Arg.Any<CancellationToken>())
            .Returns((TenantConsentPolicy?)null);

        var result = await _sut.HandleGetAsync(_tenantId, SubjectCategories.Dweller, CancellationToken.None);

        result.Should().BeNull();
    }
}

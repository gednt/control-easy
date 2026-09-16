using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Contracts;
using ControlEasyReborn.Modules.Photos.Application.Errors;
using ControlEasyReborn.Modules.Photos.Application.Handlers;
using ControlEasyReborn.Modules.Photos.Application.Validators;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Domain.Entities;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Photos;

public sealed class EntryLogHandlersTests
{
    private readonly IConsentAuditLogRepository _auditLogRepo = Substitute.For<IConsentAuditLogRepository>();
    private readonly IPhotoRepository _photoRepo = Substitute.For<IPhotoRepository>();
    private readonly ITenantConsentPolicyRepository _policyRepo = Substitute.For<ITenantConsentPolicyRepository>();
    private readonly IResidentRepository _residentRepo = Substitute.For<IResidentRepository>();
    private readonly IVehicleRepository _vehicleRepo = Substitute.For<IVehicleRepository>();
    private readonly CreateEntryLogRequestValidator _validator = new();
    private readonly CreateEntryLogHandler _createHandler;
    private readonly ListEntryLogsHandler _listHandler;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _profileId = Guid.NewGuid();

    public EntryLogHandlersTests()
    {
        _createHandler = new CreateEntryLogHandler(_auditLogRepo, _photoRepo, _policyRepo, _residentRepo, _vehicleRepo, _validator);
        _listHandler = new ListEntryLogsHandler(_auditLogRepo);
    }

    [Fact]
    public async Task CreateEntry_WhenEnteredWithConsentAndValidPhoto_Succeeds()
    {
        var photoId = Guid.NewGuid();
        var photo = new Photo(photoId, _tenantId, "2026-09/photo.jpg", null, "image/jpeg", 100, DateTime.UtcNow, DateTime.UtcNow);
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Alice",
            SubjectDocument: "123456789",
            PhotoId: photoId,
            OverrideReason: null);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.Should().NotBeNull();
        response.EntryState.Should().Be(EntryStates.EnteredWithConsent);
        response.PhotoId.Should().Be(photoId);
        response.SubjectType.Should().Be(SubjectCategories.Visitor);
        response.PerformedByProfileId.Should().Be(_profileId);

        await _auditLogRepo.Received(1).AddAsync(Arg.Is<ConsentAuditLogEntry>(e =>
            e.TenantId == _tenantId &&
            e.EntryState == EntryStates.EnteredWithConsent &&
            e.PhotoId == photoId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEntry_WhenEnteredWithConsentAndNullPhotoId_ThrowsValidationException()
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Alice",
            SubjectDocument: "123456789",
            PhotoId: null,
            OverrideReason: null);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("PhotoId"));
    }

    [Fact]
    public async Task CreateEntry_WhenPhotoNotFoundOrBelongsToAnotherTenant_ThrowsNotFoundException()
    {
        var photoId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var photo = new Photo(photoId, otherTenantId, "path", null, "image/jpeg", 100, null, DateTime.UtcNow);
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Alice",
            SubjectDocument: "123456789",
            PhotoId: photoId,
            OverrideReason: null);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(SubjectCategories.Dweller, OverrideReasons.Emergency)]
    [InlineData(SubjectCategories.Visitor, OverrideReasons.Vouched)]
    public async Task CreateEntry_WhenEnteredOverrideForDwellerOrVisitorWithReason_Succeeds(string subject, string reason)
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredOverride,
            SubjectType: subject,
            SubjectName: "Bob",
            SubjectDocument: "987654321",
            PhotoId: null,
            OverrideReason: reason);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.Should().NotBeNull();
        response.EntryState.Should().Be(EntryStates.EnteredOverride);
        response.OverrideReason.Should().Be(reason);
    }

    [Theory]
    [InlineData(SubjectCategories.ServiceProvider)]
    [InlineData(SubjectCategories.Vehicle)]
    public async Task CreateEntry_WhenEnteredOverrideForServiceProviderOrVehicle_ThrowsValidationException(string subject)
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredOverride,
            SubjectType: subject,
            SubjectName: "Contractor",
            SubjectDocument: "111222333",
            PhotoId: null,
            OverrideReason: OverrideReasons.Emergency);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("SubjectType"));
    }

    [Fact]
    public async Task CreateEntry_WhenEnteredOverrideWithoutReason_ThrowsValidationException()
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredOverride,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Bob",
            SubjectDocument: "123",
            PhotoId: null,
            OverrideReason: null);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("OverrideReason"));
    }

    [Fact]
    public async Task CreateEntry_WhenGatehouseOnlyForServiceProvider_SucceedsWithoutPhoto()
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.GatehouseOnly,
            SubjectType: SubjectCategories.ServiceProvider,
            SubjectName: "Delivery",
            SubjectDocument: "999",
            PhotoId: null,
            OverrideReason: null);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.Should().NotBeNull();
        response.EntryState.Should().Be(EntryStates.GatehouseOnly);
    }

    [Fact]
    public async Task CreateEntry_WhenGatehouseOnlyForNonServiceProvider_ThrowsValidationException()
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.GatehouseOnly,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Visitor",
            SubjectDocument: "999",
            PhotoId: null,
            OverrideReason: null);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("SubjectType"));
    }

    [Theory]
    [InlineData(SubjectCategories.Dweller)]
    [InlineData(SubjectCategories.Vehicle)]
    public async Task CreateEntry_WhenResidentOrVehicleExits_Succeeds(string subjectType)
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.Exited,
            SubjectType: subjectType,
            SubjectName: "Gatehouse subject",
            SubjectDocument: "123",
            PhotoId: null,
            OverrideReason: null);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.EntryState.Should().Be(EntryStates.Exited);
        response.SubjectType.Should().Be(subjectType);
    }

    [Fact]
    public async Task CreateEntry_WhenVisitorExits_ThrowsValidationException()
    {
        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.Exited,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Visitor",
            SubjectDocument: "123",
            PhotoId: null,
            OverrideReason: null);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("SubjectType"));
    }

    [Fact]
    public async Task CreateEntry_WhenEnteredWithoutConsentAndPolicyRequiresPhoto_ThrowsValidationException()
    {
        var policy = new TenantConsentPolicy(Guid.NewGuid(), _tenantId, SubjectCategories.Visitor, photoRequired: true, null, null, DateTime.UtcNow);
        _policyRepo.FindByCategoryAsync(_tenantId, SubjectCategories.Visitor, Arg.Any<CancellationToken>()).Returns(policy);

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithoutConsent,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Visitor Refused",
            SubjectDocument: "999",
            PhotoId: null,
            OverrideReason: null);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("PhotoId"));
    }

    [Fact]
    public async Task CreateEntry_WhenEnteredWithoutConsentUsesPhotoFromAnotherTenant_ThrowsNotFoundException()
    {
        var photoId = Guid.NewGuid();
        var policy = new TenantConsentPolicy(Guid.NewGuid(), _tenantId, SubjectCategories.Visitor, photoRequired: true, null, null, DateTime.UtcNow);
        var photo = new Photo(photoId, Guid.NewGuid(), "path", null, "image/jpeg", 100, null, DateTime.UtcNow);
        _policyRepo.FindByCategoryAsync(_tenantId, SubjectCategories.Visitor, Arg.Any<CancellationToken>()).Returns(policy);
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithoutConsent,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Visitor Refused",
            SubjectDocument: "999",
            PhotoId: photoId,
            OverrideReason: null);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ListEntryLogs_DelegatesToRepositoryAndMapsResults()
    {
        var apartmentId = Guid.NewGuid();
        var entry = new ConsentAuditLogEntry(
            id: Guid.NewGuid(),
            tenantId: _tenantId,
            entryState: EntryStates.GatehouseOnly,
            overrideReason: null,
            photoId: null,
            subjectType: SubjectCategories.ServiceProvider,
            subjectName: "Tech",
            subjectDocument: "555",
            apartmentId: apartmentId,
            performedByProfileId: _profileId,
            recordedAt: DateTime.UtcNow);

        _auditLogRepo.ListAsync(EntryStates.GatehouseOnly, null, null, null, 0, 10, Arg.Any<CancellationToken>())
            .Returns(new[] { entry });

        var results = await _listHandler.HandleAsync(EntryStates.GatehouseOnly, null, null, null, 0, 10, CancellationToken.None);

        results.Should().ContainSingle();
        results[0].Id.Should().Be(entry.Id);
        results[0].SubjectName.Should().Be("Tech");
        results[0].ApartmentId.Should().Be(apartmentId);
    }

    [Fact]
    public async Task CreateEntry_WhenDwellerHasResidentId_AttachesResidentApartmentId()
    {
        var residentId = Guid.NewGuid();
        var apartmentId = Guid.NewGuid();
        _residentRepo.FindAsync(residentId, Arg.Any<CancellationToken>()).Returns(
            new Resident(residentId, _tenantId, "Ana", "12345678909", null, null, apartmentId, true, DateTime.UtcNow));

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Dweller,
            SubjectName: "Ana",
            SubjectDocument: "12345678909",
            PhotoId: null,
            OverrideReason: null,
            ApartmentId: null,
            ResidentId: residentId);

        var act = () => _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        // Dweller entered_with_consent requires photo; ensure validation triggers before attribution
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("PhotoId"));

        // Now with photo
        var photoId = Guid.NewGuid();
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(
            new Photo(photoId, _tenantId, "2026-09/p.jpg", null, "image/jpeg", 100, DateTime.UtcNow, DateTime.UtcNow));

        var ok = await _createHandler.HandleAsync(
            request with { PhotoId = photoId }, _tenantId, _profileId, CancellationToken.None);

        ok.ApartmentId.Should().Be(apartmentId);

        await _auditLogRepo.Received(1).AddAsync(
            Arg.Is<ConsentAuditLogEntry>(e => e.ApartmentId == apartmentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEntry_WhenDwellerCpfMatches_AttachesApartmentIdFromMatch()
    {
        var apartmentId = Guid.NewGuid();
        var residentId = Guid.NewGuid();
        _residentRepo.ListAsync("12345678909", 0, 5, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new Resident(residentId, _tenantId, "Ana", "12345678909", null, null, apartmentId, true, DateTime.UtcNow)
            });

        var photoId = Guid.NewGuid();
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(
            new Photo(photoId, _tenantId, "2026-09/p.jpg", null, "image/jpeg", 100, DateTime.UtcNow, DateTime.UtcNow));

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Dweller,
            SubjectName: "Ana",
            SubjectDocument: "123.456.789-09",
            PhotoId: photoId,
            OverrideReason: null);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.ApartmentId.Should().Be(apartmentId);
        await _auditLogRepo.Received(1).AddAsync(
            Arg.Is<ConsentAuditLogEntry>(e => e.ApartmentId == apartmentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEntry_WhenVehiclePlateMatches_AttachesApartmentIdFromMatch()
    {
        var apartmentId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        _vehicleRepo.ListAsync("ABC1D23", 0, 5, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new Vehicle(vehicleId, _tenantId, "ABC1D23", null, null, null, apartmentId, "Driver", VehicleType.Car, true, DateTime.UtcNow)
            });

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Vehicle,
            SubjectName: "Driver",
            SubjectDocument: "abc1d23",
            PhotoId: null,
            OverrideReason: null);

        // Vehicle + entered_with_consent still requires a photo
        var photoId = Guid.NewGuid();
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(
            new Photo(photoId, _tenantId, "2026-09/p.jpg", null, "image/jpeg", 100, DateTime.UtcNow, DateTime.UtcNow));

        var response = await _createHandler.HandleAsync(
            request with { PhotoId = photoId }, _tenantId, _profileId, CancellationToken.None);

        response.ApartmentId.Should().Be(apartmentId);
    }

    [Fact]
    public async Task CreateEntry_WhenVehicleExitsByPlate_AttachesApartmentId()
    {
        var apartmentId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        _vehicleRepo.ListAsync("XYZ9A88", 0, 5, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new Vehicle(vehicleId, _tenantId, "XYZ9A88", null, null, null, apartmentId, "Driver", VehicleType.Car, true, DateTime.UtcNow)
            });

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.Exited,
            SubjectType: SubjectCategories.Vehicle,
            SubjectName: "Driver",
            SubjectDocument: "xyz9a88",
            PhotoId: null,
            OverrideReason: null);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.ApartmentId.Should().Be(apartmentId);
        response.EntryState.Should().Be(EntryStates.Exited);
    }

    [Fact]
    public async Task CreateEntry_WhenNoMatch_StoresNullApartmentId()
    {
        var photoId = Guid.NewGuid();
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(
            new Photo(photoId, _tenantId, "2026-09/p.jpg", null, "image/jpeg", 100, DateTime.UtcNow, DateTime.UtcNow));

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Dweller,
            SubjectName: "Unknown",
            SubjectDocument: "00000000000",
            PhotoId: photoId,
            OverrideReason: null);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.ApartmentId.Should().BeNull();
    }

    [Fact]
    public async Task CreateEntry_WhenExplicitApartmentId_KeepsIt()
    {
        var photoId = Guid.NewGuid();
        var explicitApartment = Guid.NewGuid();
        _photoRepo.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(
            new Photo(photoId, _tenantId, "2026-09/p.jpg", null, "image/jpeg", 100, DateTime.UtcNow, DateTime.UtcNow));

        var request = new CreateEntryLogRequest(
            EntryState: EntryStates.EnteredWithConsent,
            SubjectType: SubjectCategories.Visitor,
            SubjectName: "Visitor",
            SubjectDocument: null,
            PhotoId: photoId,
            OverrideReason: null,
            ApartmentId: explicitApartment);

        var response = await _createHandler.HandleAsync(request, _tenantId, _profileId, CancellationToken.None);

        response.ApartmentId.Should().Be(explicitApartment);
    }
}

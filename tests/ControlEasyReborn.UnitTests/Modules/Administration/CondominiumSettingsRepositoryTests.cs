using System.Data;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.Modules.Administration.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using DBTools.Abstractions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Administration;

public sealed class CondominiumSettingsRepositoryTests
{
    private readonly Guid _tenantId = Guid.Parse("42a0c6db-6f20-4041-b24d-30bcad5d891b");
    private readonly FakeAsyncSqlClient _fakeClient;
    private readonly CondominiumSettingsRepository _sut;

    public CondominiumSettingsRepositoryTests()
    {
        _fakeClient = new FakeAsyncSqlClient();
        _fakeClient.AddInterceptor(new TenantFilterInterceptor(_tenantId));
        var factory = new FakeTenantAwareLinqFactory(_fakeClient);
        var ctx = new FakeTenantContext(_tenantId);
        _sut = new CondominiumSettingsRepository(ctx, factory);
    }

    [Fact]
    public async Task GetByTenantIdAsync_should_query_by_tenant_id()
    {
        _fakeClient.SelectResultFactory = () => EmptySettingsTable();

        var result = await _sut.GetByTenantIdAsync(_tenantId, CancellationToken.None);

        result.Should().BeNull();
        _fakeClient.Operations.Should().ContainSingle();
        var op = _fakeClient.Operations[0];
        op.OperationType.Should().Be("Select");
        op.Sql.Should().Contain($"TenantId = '{_tenantId}'");
    }

    [Fact]
    public async Task SaveAsync_when_no_existing_row_should_insert_including_tenant_id()
    {
        _fakeClient.SelectResultFactory = () => EmptySettingsTable();

        var settings = CondominiumSettings.CreateDefault(_tenantId);
        await _sut.SaveAsync(settings, CancellationToken.None);

        var insertOp = _fakeClient.Operations.FirstOrDefault(o => o.OperationType == "Insert");
        insertOp.Should().NotBeNull();
        insertOp!.Sql.Should().Contain("tenant_id");
        insertOp.Parameters.Should().Contain(_tenantId);
    }

    [Fact]
    public async Task SaveAsync_when_existing_row_exists_should_update_correctly()
    {
        _fakeClient.SelectResultFactory = () => PopulatedSettingsTable(_tenantId);

        var settings = CondominiumSettings.CreateDefault(_tenantId);
        settings.Update(
            visitDurationMinutes: 180,
            requireShiftHandoverNotes: false,
            defaultShiftLengthHours: 12,
            emergencyContactPhone: "555-1234",
            allowedVisitorStartHour: "07:00",
            allowedVisitorEndHour: "23:00",
            autoCheckoutAtMidnight: false,
            maxActiveVisitorsPerUnit: 8,
            photoRequiredVisitors: false,
            photoRequiredProviders: true,
            photoRequiredResidents: true,
            allowOverrideOnRefusal: false,
            overdueVisitAlertMinutes: 30);

        await _sut.SaveAsync(settings, CancellationToken.None);

        var updateOp = _fakeClient.Operations.FirstOrDefault(o => o.OperationType == "Update");
        updateOp.Should().NotBeNull();
        updateOp!.Sql.Should().Contain($"TenantId = '{_tenantId}'");
        updateOp.Sql.Should().Contain("VisitDurationMinutes");
    }

    [Fact]
    public async Task GetByTenantIdAsync_should_map_all_properties()
    {
        _fakeClient.SelectResultFactory = () => PopulatedSettingsTable(_tenantId);

        var result = await _sut.GetByTenantIdAsync(_tenantId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TenantId.Should().Be(_tenantId);
        result.VisitDurationMinutes.Should().Be(120);
        result.RequireShiftHandoverNotes.Should().BeTrue();
        result.DefaultShiftLengthHours.Should().Be(8);
        result.AllowedVisitorStartHour.Should().Be("06:00");
        result.AllowedVisitorEndHour.Should().Be("22:00");
        result.AutoCheckoutAtMidnight.Should().BeTrue();
        result.MaxActiveVisitorsPerUnit.Should().Be(5);
        result.PhotoRequiredVisitors.Should().BeTrue();
        result.PhotoRequiredProviders.Should().BeTrue();
        result.PhotoRequiredResidents.Should().BeFalse();
        result.AllowOverrideOnRefusal.Should().BeTrue();
        result.OverdueVisitAlertMinutes.Should().Be(15);
    }

    private static DataTable EmptySettingsTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(string));
        dt.Columns.Add("TenantId", typeof(string));
        dt.Columns.Add("tenant_id", typeof(string));
        dt.Columns.Add("VisitDurationMinutes", typeof(int));
        dt.Columns.Add("RequireShiftHandoverNotes", typeof(bool));
        dt.Columns.Add("DefaultShiftLengthHours", typeof(int));
        dt.Columns.Add("EmergencyContactPhone", typeof(string));
        dt.Columns.Add("AllowedVisitorStartHour", typeof(string));
        dt.Columns.Add("AllowedVisitorEndHour", typeof(string));
        dt.Columns.Add("AutoCheckoutAtMidnight", typeof(bool));
        dt.Columns.Add("MaxActiveVisitorsPerUnit", typeof(int));
        dt.Columns.Add("PhotoRequiredVisitors", typeof(bool));
        dt.Columns.Add("PhotoRequiredProviders", typeof(bool));
        dt.Columns.Add("PhotoRequiredResidents", typeof(bool));
        dt.Columns.Add("AllowOverrideOnRefusal", typeof(bool));
        dt.Columns.Add("OverdueVisitAlertMinutes", typeof(int));
        dt.Columns.Add("CreatedAtUtc", typeof(DateTime));
        dt.Columns.Add("UpdatedAtUtc", typeof(string));
        return dt;
    }

    private static DataTable PopulatedSettingsTable(Guid tenantId)
    {
        var dt = EmptySettingsTable();
        dt.Rows.Add(
            Guid.NewGuid().ToString(),
            tenantId.ToString(),
            tenantId.ToString(),
            120,
            true,
            8,
            null,
            "06:00",
            "22:00",
            true,
            5,
            true,
            true,
            false,
            true,
            15,
            DateTime.UtcNow,
            null);
        return dt;
    }

    private sealed class FakeTenantAwareLinqFactory : ITenantAwareLinqFactory
    {
        private readonly FakeAsyncSqlClient _client;
        public FakeTenantAwareLinqFactory(FakeAsyncSqlClient client) { _client = client; }
        public IAsyncSqlClient Create(ITenantContext ctx, bool bypassTenantFilter = false) => _client;
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public FakeTenantContext(Guid tenantId) { TenantId = tenantId; }
        public Guid? TenantId { get; }
        public Guid? ProfileId => null;
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
        public bool IsPlatformAdmin => false;
        public bool IsResolved => true;
        public void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions) { }
    }
}

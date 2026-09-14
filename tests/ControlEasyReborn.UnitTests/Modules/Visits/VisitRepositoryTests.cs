using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Visits.Domain.Entities;
using ControlEasyReborn.Modules.Visits.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using DBTools.Abstractions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Visits;

public sealed class VisitRepositoryTests
{
    private readonly Guid _tenantId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private readonly FakeAsyncSqlClient _fakeClient;
    private readonly VisitRepository _sut;

    public VisitRepositoryTests()
    {
        _fakeClient = new FakeAsyncSqlClient();
        _fakeClient.AddInterceptor(new TenantFilterInterceptor(_tenantId));
        var factory = new FakeTenantAwareLinqFactory(_fakeClient);
        var ctx = new FakeTenantContext(_tenantId);
        _sut = new VisitRepository(ctx, factory);
    }

    [Fact]
    public async Task UpdateAsync_uses_id_parameter_after_set_fields()
    {
        var visitId = Guid.NewGuid();
        var visit = new Visit(
            id: visitId,
            tenantId: _tenantId,
            visitorName: "Visitor",
            visitorDocument: "DOC1",
            visitorPhone: null,
            apartmentId: null,
            purpose: null,
            status: VisitStatus.CheckedIn,
            attendantProfileId: Guid.Parse("00000000-0000-0000-0000-000000000301"),
            gatehouseId: null,
            checkedInAtUtc: DateTime.UtcNow,
            checkedOutAtUtc: null,
            createdAtUtc: DateTime.UtcNow,
            updatedAtUtc: DateTime.UtcNow);

        await _sut.UpdateAsync(visit, CancellationToken.None);

        var op = _fakeClient.Operations.Should().ContainSingle().Which;
        op.OperationType.Should().Be("Update");
        op.Sql.Should().Contain($"Id = '{visitId}'");
        op.Parameters.Should().NotContain(visitId);
    }

    [Fact]
    public async Task ListAsync_with_status_filter_uses_param0()
    {
        await _sut.ListAsync(_tenantId, "Pending", 0, 10, CancellationToken.None);

        var op = _fakeClient.Operations
            .Where(o => o.OperationType == "Select")
            .Should()
            .ContainSingle()
            .Which;
        op.Sql.Should().Contain("Status = @param0");
        op.Parameters[0].Should().Be(0);
    }

    private sealed class FakeTenantAwareLinqFactory : ITenantAwareLinqFactory
    {
        private readonly FakeAsyncSqlClient _client;

        public FakeTenantAwareLinqFactory(FakeAsyncSqlClient client) => _client = client;

        public IAsyncSqlClient Create(ITenantContext ctx, bool bypassTenantFilter = false) => _client;
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public FakeTenantContext(Guid tenantId) => TenantId = tenantId;

        public Guid? TenantId { get; }
        public Guid? ProfileId => Guid.Parse("00000000-0000-0000-0000-000000000301");
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
        public bool IsPlatformAdmin => false;
        public bool IsResolved => true;

        public void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions) { }
    }
}

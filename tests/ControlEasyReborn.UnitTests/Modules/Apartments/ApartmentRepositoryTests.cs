using System.Data;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Apartments.Domain.Entities;
using ControlEasyReborn.Modules.Apartments.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using DBTools.Abstractions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Apartments;

public sealed class ApartmentRepositoryTests
{
    private readonly Guid _tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly FakeAsyncSqlClient _fakeClient;
    private readonly ApartmentRepository _sut;

    public ApartmentRepositoryTests()
    {
        _fakeClient = new FakeAsyncSqlClient();
        _fakeClient.AddInterceptor(new TenantFilterInterceptor(_tenantId));
        var factory = new FakeTenantAwareLinqFactory(_fakeClient);
        var ctx = new FakeTenantContext(_tenantId);
        _sut = new ApartmentRepository(ctx, factory);
    }

    [Fact]
    public async Task FindAsync_sends_select_with_id_parameter()
    {
        _fakeClient.SelectResultFactory = () => EmptyApartmentsTable();

        var id = Guid.NewGuid();
        await _sut.FindAsync(id, CancellationToken.None);

        _fakeClient.Operations.Should().ContainSingle();
        var op = _fakeClient.Operations[0];
        op.OperationType.Should().Be("Select");
        op.Sql.Should().Contain("Id = @param0");
        op.Parameters.Should().Contain(id);
    }

    [Fact]
    public async Task FindByBlockUnitAsync_sends_block_and_unit_predicate()
    {
        _fakeClient.SelectResultFactory = () => EmptyApartmentsTable();

        await _sut.FindByBlockUnitAsync("A", "101", CancellationToken.None);

        var op = _fakeClient.Operations[0];
        op.Sql.Should().Contain("Block = @param0 AND Unit = @param1");
        op.Parameters.Should().Contain("A");
        op.Parameters.Should().Contain("101");
    }

    [Fact]
    public async Task ListAsync_with_search_sends_like_predicate()
    {
        _fakeClient.SelectResultFactory = () => EmptyApartmentsTable();

        await _sut.ListAsync("Tower", 0, 50, CancellationToken.None);

        var op = _fakeClient.Operations[0];
        op.Sql.Should().Contain("Block LIKE @param0 OR Unit LIKE @param0");
        op.Parameters.Should().Contain("%Tower%");
    }

    [Fact]
    public async Task AddAsync_sends_insert_with_apartment_fields()
    {
        var apartment = new Apartment(
            id: Guid.NewGuid(),
            tenantId: _tenantId,
            block: "A",
            unit: "101",
            active: true,
            createdAtUtc: DateTime.UtcNow);

        await _sut.AddAsync(apartment, CancellationToken.None);

        var op = _fakeClient.Operations[0];
        op.OperationType.Should().Be("Insert");
        op.Sql.Should().Contain("INSERT INTO Apartments");
        op.Parameters.Should().Contain(apartment.Id);
    }

    [Fact]
    public async Task ExistsAsync_returns_true_when_row_found()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        _fakeClient.SelectResultFactory = () =>
        {
            var dt = EmptyApartmentsTable();
            dt.Rows.Add(id.ToString(), _tenantId.ToString(), "A", "101", 1, now, DBNull.Value);
            return dt;
        };

        var result = await _sut.ExistsAsync(id, CancellationToken.None);
        result.Should().BeTrue();
    }

    private static DataTable EmptyApartmentsTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(string));
        dt.Columns.Add("TenantId", typeof(string));
        dt.Columns.Add("Block", typeof(string));
        dt.Columns.Add("Unit", typeof(string));
        dt.Columns.Add("Active", typeof(int));
        dt.Columns.Add("CreatedAtUtc", typeof(DateTime));
        dt.Columns.Add("UpdatedAtUtc", typeof(object));
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

using System.Data;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Residents.Domain.Entities;
using ControlEasyReborn.Modules.Residents.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using DBTools.Abstractions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Residents;

public sealed class ResidentRepositoryTests
{
    private readonly Guid _tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly FakeAsyncSqlClient _fakeClient;
    private readonly ResidentRepository _sut;

    public ResidentRepositoryTests()
    {
        _fakeClient = new FakeAsyncSqlClient();
        _fakeClient.AddInterceptor(new TenantFilterInterceptor(_tenantId));
        var factory = new FakeTenantAwareLinqFactory(_fakeClient);
        var ctx = new FakeTenantContext(_tenantId);
        _sut = new ResidentRepository(ctx, factory);
    }

    [Fact]
    public async Task FindAsync_sends_select_with_id_parameter()
    {
        _fakeClient.SelectResultFactory = () => EmptyResidentsTable();

        var id = Guid.NewGuid();
        await _sut.FindAsync(id, CancellationToken.None);

        _fakeClient.Operations.Should().ContainSingle();
        var op = _fakeClient.Operations[0];
        op.OperationType.Should().Be("Select");
        op.Sql.Should().Contain("Id = @param0");
        op.Parameters.Should().Contain(id);
    }

    [Fact]
    public async Task FindAsync_select_sql_contains_tenant_filter()
    {
        _fakeClient.SelectResultFactory = () => EmptyResidentsTable();

        await _sut.FindAsync(Guid.NewGuid(), CancellationToken.None);

        _fakeClient.Operations[0].Sql.Should().Contain("tenant_id = @ctx_tenant");
        _fakeClient.Operations[0].Parameters.Should().Contain(_tenantId);
    }

    [Fact]
    public async Task ListAsync_without_search_sends_unfiltered_select()
    {
        _fakeClient.SelectResultFactory = () => EmptyResidentsTable();

        await _sut.ListAsync(null, 0, 50, CancellationToken.None);

        _fakeClient.Operations.Should().ContainSingle();
        var op = _fakeClient.Operations[0];
        op.OperationType.Should().Be("Select");
        op.Sql.Should().Contain("1=1");
    }

    [Fact]
    public async Task ListAsync_with_search_sends_like_predicate()
    {
        _fakeClient.SelectResultFactory = () => EmptyResidentsTable();

        await _sut.ListAsync("Maria", 0, 50, CancellationToken.None);

        var op = _fakeClient.Operations[0];
        op.Sql.Should().Contain("Name LIKE @param0");
        op.Parameters.Should().Contain("%Maria%");
    }

    [Fact]
    public async Task AddAsync_sends_insert_with_resident_fields()
    {
        var resident = new Resident(
            id: Guid.NewGuid(),
            tenantId: _tenantId,
            name: "Maria Silva",
            cpf: "52998224725",
            email: "maria@test.com",
            phone: "11999990001",
            apartmentId: null,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        await _sut.AddAsync(resident, CancellationToken.None);

        _fakeClient.Operations.Should().ContainSingle();
        var op = _fakeClient.Operations[0];
        op.OperationType.Should().Be("Insert");
        op.Sql.Should().Contain("INSERT INTO Residents");
        op.Parameters.Should().Contain(resident.Id);
    }

    [Fact]
    public async Task UpdateAsync_sends_update_with_resident_fields()
    {
        var resident = new Resident(
            id: Guid.NewGuid(),
            tenantId: _tenantId,
            name: "Maria Silva",
            cpf: "52998224725",
            email: "maria@test.com",
            phone: "11999990001",
            apartmentId: null,
            active: true,
            createdAtUtc: DateTime.UtcNow,
            updatedAtUtc: DateTime.UtcNow);

        await _sut.UpdateAsync(resident, CancellationToken.None);

        _fakeClient.Operations.Should().ContainSingle();
        var op = _fakeClient.Operations[0];
        op.OperationType.Should().Be("Update");
        op.Sql.Should().Contain("UPDATE Residents");
        op.Parameters.Should().Contain(resident.Id);
    }

    [Fact]
    public async Task FindAsync_returns_null_when_no_rows()
    {
        _fakeClient.SelectResultFactory = () => EmptyResidentsTable();

        var result = await _sut.FindAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_returns_resident_when_row_found()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        _fakeClient.SelectResultFactory = () =>
        {
            var dt = EmptyResidentsTable();
            dt.Rows.Add(id.ToString(), _tenantId.ToString(), "Maria", "52998224725", "maria@test.com", "11999990001", DBNull.Value, 1, now, DBNull.Value);
            return dt;
        };

        var result = await _sut.FindAsync(id, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Maria");
        result.Cpf.Should().Be("52998224725");
    }

    [Fact]
    public async Task ListAsync_returns_empty_when_no_rows()
    {
        _fakeClient.SelectResultFactory = () => EmptyResidentsTable();

        var result = await _sut.ListAsync(null, 0, 50, CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_returns_mapped_residents()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        _fakeClient.SelectResultFactory = () =>
        {
            var dt = EmptyResidentsTable();
            dt.Rows.Add(id.ToString(), _tenantId.ToString(), "Maria", "52998224725", "maria@test.com", "11999990001", DBNull.Value, 1, now, DBNull.Value);
            return dt;
        };

        var result = await _sut.ListAsync(null, 0, 50, CancellationToken.None);
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(id);
    }

    private static DataTable EmptyResidentsTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(string));
        dt.Columns.Add("TenantId", typeof(string));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Cpf", typeof(string));
        dt.Columns.Add("Email", typeof(string));
        dt.Columns.Add("Phone", typeof(string));
        dt.Columns.Add("ApartmentId", typeof(string));
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
using System.Data;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.Modules.Administration.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.Auditing;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Administration;

public sealed class AuditLogRepositoryTests
{
    private readonly Guid _tenantId = Guid.Parse("42a0c6db-6f20-4041-b24d-30bcad5d891b");
    private readonly FakeAsyncSqlClient _fakeClient;
    private readonly AuditLogRepository _sut;

    public AuditLogRepositoryTests()
    {
        _fakeClient = new FakeAsyncSqlClient();
        _fakeClient.AddInterceptor(new TenantFilterInterceptor(_tenantId));
        _sut = new AuditLogRepository(new FakeTenantContext(_tenantId), new FakeTenantAwareLinqFactory(_fakeClient));
    }

    [Fact]
    public async Task ListAsync_should_leave_ordering_out_of_tenant_filtered_predicate_and_page_newest_first()
    {
        var olderId = Guid.NewGuid();
        var newerId = Guid.NewGuid();
        _fakeClient.SelectResultFactory = () => AuditLogTable(
            CreateRow(olderId, _tenantId, new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc)),
            CreateRow(newerId, _tenantId, new DateTime(2026, 9, 13, 10, 0, 0, DateTimeKind.Utc)));

        var result = await _sut.ListAsync(
            _tenantId,
            category: "Security",
            severity: AuditSeverity.Warning,
            entityType: "Resident",
            action: "AccessDenied",
            fromUtc: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            toUtc: new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc),
            searchTerm: "entry",
            skip: 1,
            take: 1,
            ct: CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(olderId);

        var operation = _fakeClient.Operations.Should().ContainSingle().Subject;
        operation.Sql.Should().Contain("TenantId = @param0");
        operation.Sql.Should().Contain("Category = @param1");
        operation.Sql.Should().Contain("Severity = @param2");
        operation.Sql.Should().Contain("EntityType = @param3");
        operation.Sql.Should().Contain("Action = @param4");
        operation.Sql.Should().Contain("CreatedAtUtc >= @param5");
        operation.Sql.Should().Contain("CreatedAtUtc <= @param6");
        operation.Sql.Should().Contain("Action LIKE @param7");
        operation.Sql.Should().Contain("tenant_id = @param8");
        operation.Parameters.Should().Equal(
            _tenantId,
            "Security",
            (int)AuditSeverity.Warning,
            "Resident",
            "AccessDenied",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc),
            "%entry%",
            _tenantId);
        operation.Sql.Should().NotContain("ORDER BY", "the tenant interceptor appends a predicate to this SQL");
    }

    [Fact]
    public async Task GetByEntityAsync_should_apply_tenant_filter_before_sorting_and_paging()
    {
        var olderId = Guid.NewGuid();
        var newerId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        _fakeClient.SelectResultFactory = () => AuditLogTable(
            CreateRow(olderId, _tenantId, new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc), entityId),
            CreateRow(newerId, _tenantId, new DateTime(2026, 9, 13, 10, 0, 0, DateTimeKind.Utc), entityId));

        var result = await _sut.GetByEntityAsync(entityId, skip: 0, take: 1, CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be(newerId);

        var operation = _fakeClient.Operations.Should().ContainSingle().Subject;
        operation.Sql.Should().Contain("EntityId = @param0 AND tenant_id = @param1");
        operation.Parameters.Should().Contain(_tenantId);
        operation.Sql.Should().NotContain("ORDER BY", "the tenant interceptor must be able to append its predicate safely");
    }

    private static object?[] CreateRow(Guid id, Guid tenantId, DateTime createdAtUtc, Guid? entityId = null)
    {
        return
        [
            id.ToString(), tenantId.ToString(), tenantId.ToString(), "Security", "AccessDenied", "Resident",
            (entityId ?? Guid.NewGuid()).ToString(), (int)AuditSeverity.Warning, Guid.NewGuid().ToString(), "Gatehouse", "Entry denied",
            DBNull.Value, createdAtUtc
        ];
    }

    private static DataTable AuditLogTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("TenantId", typeof(string));
        table.Columns.Add("tenant_id", typeof(string));
        table.Columns.Add("Category", typeof(string));
        table.Columns.Add("Action", typeof(string));
        table.Columns.Add("EntityType", typeof(string));
        table.Columns.Add("EntityId", typeof(string));
        table.Columns.Add("Severity", typeof(int));
        table.Columns.Add("PerformedByUserId", typeof(string));
        table.Columns.Add("PerformedByName", typeof(string));
        table.Columns.Add("Details", typeof(string));
        table.Columns.Add("MetadataJson", typeof(string));
        table.Columns.Add("CreatedAtUtc", typeof(DateTime));

        foreach (var row in rows)
        {
            table.Rows.Add(row);
        }

        return table;
    }

    private sealed class FakeTenantAwareLinqFactory(FakeAsyncSqlClient client) : ITenantAwareLinqFactory
    {
        public DBTools.Abstractions.IAsyncSqlClient Create(ITenantContext ctx, bool bypassTenantFilter = false) => client;
    }

    private sealed class FakeTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;
        public Guid? ProfileId => null;
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
        public bool IsPlatformAdmin => false;
        public bool IsResolved => true;
        public void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions) { }
    }
}

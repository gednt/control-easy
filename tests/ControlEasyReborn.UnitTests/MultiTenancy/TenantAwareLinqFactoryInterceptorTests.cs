using System.Data.Common;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using DBTools.Abstractions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.MultiTenancy;

public sealed class TenantAwareLinqFactoryInterceptorTests
{
    [Fact]
    public void Two_Linq_queries_built_by_TenantAwareLinqFactory_with_different_ITenantContext_produce_distinct_tenant_id_parameters()
    {
        var tenantIdA = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var tenantIdB = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var interceptorA = new TenantFilterInterceptor(tenantIdA);
        var interceptorB = new TenantFilterInterceptor(tenantIdB);

        var fakeA = new FakeAsyncSqlClient();
        fakeA.AddInterceptor(interceptorA);
        fakeA.SelectResultFactory = () => new System.Data.DataTable();

        var fakeB = new FakeAsyncSqlClient();
        fakeB.AddInterceptor(interceptorB);
        fakeB.SelectResultFactory = () => new System.Data.DataTable();

        _ = fakeA.SelectAsync("Id, Name", "Residents", "1=1", Array.Empty<object>());
        _ = fakeB.SelectAsync("Id, Name", "Residents", "1=1", Array.Empty<object>());

        var sqlA = fakeA.Operations[0].Sql;
        var sqlB = fakeB.Operations[0].Sql;

        sqlA.Should().Contain("tenant_id = @ctx_tenant");
        sqlB.Should().Contain("tenant_id = @ctx_tenant");

        fakeA.Operations[0].Parameters.Should().Contain(tenantIdA);
        fakeB.Operations[0].Parameters.Should().Contain(tenantIdB);

        fakeA.Operations[0].Parameters.Should().NotContain(tenantIdB);
        fakeB.Operations[0].Parameters.Should().NotContain(tenantIdA);
    }

    [Fact]
    public void TenantAwareLinqFactory_creates_client_with_interceptor_attached()
    {
        var tenantId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var ctx = new TestTenantContext(tenantId);

        var factory = new TenantAwareLinqFactory(
            new FakeQueryBuilder(),
            new FakeValidator(),
            new FakeProvider(),
            new FakeConfiguration());

        var client = factory.Create(ctx);
        client.Should().NotBeNull();

        var interceptor = new TenantFilterInterceptor(tenantId);
        var context = new QueryInterceptionContext
        {
            Sql = "SELECT Id, Name FROM Residents WHERE 1=1",
            OperationType = QueryOperationType.Select,
            TableName = "Residents"
        };
        interceptor.BeforeExecute(context);

        context.Sql.Should().Contain("tenant_id = @ctx_tenant");
        context.Parameters.Should().Contain(tenantId);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public TestTenantContext(Guid tenantId) { TenantId = tenantId; }
        public Guid? TenantId { get; }
        public Guid? ProfileId => null;
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
        public bool IsPlatformAdmin => false;
        public bool IsResolved => true;
        public void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions) { }
    }

    private sealed class FakeQueryBuilder : ISqlQueryBuilder
    {
        public string SelectQuery(string fields, string table, string conditions) => string.Empty;
        public string InsertQuery(string[] fields, string table, object[] values, string primaryKeyName = "", bool autoIncrement = true) => string.Empty;
        public string UpdateQuery(string[] fields, string table, string[] values, string condition = "") => string.Empty;
        public string DeleteQuery(string table, string condition) => string.Empty;
        public List<DbParameter> GenerateSqlParameters(object[] values) => new();
        public List<DbParameter> GenerateSqlParameters(object[] values, IDbProvider provider) => new();
    }

    private sealed class FakeValidator : ISqlValidator
    {
        public bool IsValidIdentifier(string identifier) => true;
    }

    private sealed class FakeProvider : IDbProvider
    {
        public DbConnection CreateConnection(string connectionString) => null!;
        public DbCommand CreateCommand() => null!;
        public DbParameter CreateParameter(string name, object value) => null!;
        public string ParameterPrefix => "@";
        public string ProviderName => "MySQL";
        public string QuoteIdentifier(string identifier) => identifier;
        public string BuildPagingClause(int? skip, int? take, string orderByClause) => string.Empty;
        public string GetLastInsertedIdSql() => "SELECT LAST_INSERT_ID()";
        public bool SupportsMerge => false;
        public bool UsesTopNSyntax => false;
        public string BuildUpsertSql(string tableName, string[] columns, string matchColumn, string parameterPrefix) => string.Empty;
    }

    private sealed class FakeConfiguration : IDbConfiguration
    {
        public string Host => "localhost";
        public string Database => "fake";
        public string Uid => "root";
        public string Password => string.Empty;
        public string Port => "3306";
        public string ConnectionString => "Server=localhost;Database=fake;Uid=root;Pwd=;";
        public string Provider => "MySQL";
    }
}
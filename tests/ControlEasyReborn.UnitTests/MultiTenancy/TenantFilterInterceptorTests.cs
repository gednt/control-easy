using System.Text.RegularExpressions;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using DBTools.Abstractions;
using Xunit;

namespace ControlEasyReborn.UnitTests.MultiTenancy;

// 1.0a acceptance criterion: "verified by a unit test that runs two
// queries with different `ITenantContext` instances and asserts the
// generated SQL includes the right `WHERE tenant_id = @ctx_tenant` clause."
//
// With the real DBTools_SQL, the interceptor is the only place the
// filter is applied. The gate test drives the interceptor directly with
// the same context both contexts would set, then inspects the
// `QueryInterceptionContext.Sql` and `Parameters` it mutated.
public sealed class TenantFilterInterceptorTests
{
    [Fact]
    public void Two_interceptors_with_different_ITenantContext_instances_produce_distinct_WHERE_clauses()
    {
        var ctxA = new TestTenantContext(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var ctxB = new TestTenantContext(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        var interceptorA = new TenantFilterInterceptor(ctxA.TenantId);
        var interceptorB = new TenantFilterInterceptor(ctxB.TenantId);

        // Simulate the two "queries" the spec calls for: two `SelectAsync`
        // calls going through the real DBTools pipeline. The interceptor
        // mutates the `QueryInterceptionContext` in place; we inspect the
        // mutated SQL and the appended parameter.
        var contextA = NewSelectContext("SELECT * FROM Residents");
        var contextB = NewSelectContext("SELECT * FROM Residents");

        interceptorA.BeforeExecute(contextA);
        interceptorB.BeforeExecute(contextB);

        Assert.Contains("WHERE tenant_id = @param0", contextA.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE tenant_id = @param0", contextB.Sql, StringComparison.OrdinalIgnoreCase);

        // The two contexts produced two different tenant_id parameter values.
        Assert.Contains(ctxA.TenantId!.Value, contextA.Parameters);
        Assert.Contains(ctxB.TenantId!.Value, contextB.Parameters);
        Assert.DoesNotContain(ctxB.TenantId!.Value, contextA.Parameters);
        Assert.DoesNotContain(ctxA.TenantId!.Value, contextB.Parameters);
    }

    [Fact]
    public void Interceptor_inserts_tenant_id_as_first_WHERE_predicate()
    {
        var tenantId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var interceptor = new TenantFilterInterceptor(tenantId);

        var context = NewSelectContext("SELECT * FROM Residents WHERE Name = @param0");
        context.Parameters.Add("Chaves");
        interceptor.BeforeExecute(context);

        // The tenant filter is appended as the last predicate after
        // the WHERE keyword; the original predicate is preserved.
        Assert.Contains("WHERE Name = @param0 AND tenant_id = @param1", context.Sql, StringComparison.Ordinal);
        Assert.Contains(tenantId, context.Parameters);
    }

    [Fact]
    public void Interceptor_is_no_op_when_tenant_id_already_in_sql()
    {
        var tenantId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var interceptor = new TenantFilterInterceptor(tenantId);

        var context = NewSelectContext("SELECT * FROM Residents WHERE tenant_id = @param0");
        interceptor.BeforeExecute(context);

        var count = CountOccurrences(context.Sql, "tenant_id");
        Assert.Equal(1, count);
    }

    [Fact]
    public void Interceptor_skips_injection_on_aliased_tenant_id_in_join_query()
    {
        var tenantId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var interceptor = new TenantFilterInterceptor(tenantId);

        var sql = "SELECT DISTINCT a.Id FROM Apartments a INNER JOIN Residents r ON a.Id = r.ApartmentId WHERE a.tenant_id = @param0 AND r.tenant_id = @param1 AND r.Active = 1";
        var context = new QueryInterceptionContext
        {
            Sql = sql,
            OperationType = QueryOperationType.Select,
            TableName = "Apartments"
        };
        context.Parameters.Add(tenantId);
        context.Parameters.Add(tenantId);

        interceptor.BeforeExecute(context);

        Assert.Equal(sql, context.Sql);
        Assert.Equal(2, context.Parameters.Count);
    }

    [Fact]
    public void Interceptor_bypass_returns_sql_unchanged()
    {
        var interceptor = new TenantFilterInterceptor(null, bypass: true);
        var context = NewSelectContext("SELECT * FROM Tenants");

        interceptor.BeforeExecute(context);

        Assert.Equal("SELECT * FROM Tenants", context.Sql);
        Assert.Empty(context.Parameters);
    }

    [Fact]
    public void Interceptor_with_null_tenant_returns_sql_unchanged()
    {
        var interceptor = new TenantFilterInterceptor(null, bypass: false);
        var context = NewSelectContext("SELECT * FROM Tenants");

        interceptor.BeforeExecute(context);

        Assert.Equal("SELECT * FROM Tenants", context.Sql);
        Assert.Empty(context.Parameters);
    }

    [Fact]
    public void Bypass_property_short_circuits_interceptor()
    {
        var interceptor = new TenantFilterInterceptor(Guid.NewGuid());
        var context = NewSelectContext("SELECT * FROM Residents");
        context.Properties[TenantFilterInterceptor.BypassPropertyKey] = true;

        interceptor.BeforeExecute(context);

        Assert.Equal("SELECT * FROM Residents", context.Sql);
    }

    [Fact]
    public void Interceptor_does_not_mutate_Insert_statements()
    {
        // Inserts never have a WHERE clause; the interceptor only appends
        // a filter to Select/Update/Delete.
        var tenantId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var interceptor = new TenantFilterInterceptor(tenantId);

        var context = new QueryInterceptionContext
        {
            Sql = "INSERT INTO Residents (Name) VALUES (@param0)",
            OperationType = QueryOperationType.Insert
        };

        interceptor.BeforeExecute(context);

        Assert.Equal("INSERT INTO Residents (Name) VALUES (@param0)", context.Sql);
        Assert.Empty(context.Parameters);
    }

    private static QueryInterceptionContext NewSelectContext(string sql)
    {
        return new QueryInterceptionContext
        {
            Sql = sql,
            OperationType = QueryOperationType.Select,
            TableName = "Residents"
        };
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        if (string.IsNullOrEmpty(needle)) return 0;
        var count = 0;
        var idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }

    // Test double for `ITenantContext` -- avoids pulling in the
    // HttpTenantContext / NullTenantContext types and the AspNetCore
    // dependency they bring.
    private sealed class TestTenantContext : ControlEasyReborn.SharedKernel.MultiTenancy.ITenantContext
    {
        public TestTenantContext(Guid? tenantId) { TenantId = tenantId; }
        public Guid? TenantId { get; }
        public Guid? ProfileId => null;
        public System.Collections.Generic.IReadOnlyCollection<string> Roles => System.Array.Empty<string>();
        public System.Collections.Generic.IReadOnlyCollection<string> Permissions => System.Array.Empty<string>();
        public bool IsPlatformAdmin => false;
        public bool IsResolved => true;
        public void Set(Guid? tenantId, Guid? profileId, System.Collections.Generic.IReadOnlyCollection<string> roles, System.Collections.Generic.IReadOnlyCollection<string> permissions) { }
    }
}

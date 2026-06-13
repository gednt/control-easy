using System.Collections.Generic;
using DBTools.Abstractions;

namespace ControlEasyReborn.Infrastructure.MultiTenancy;

// Appends a `WHERE tenant_id = @ctx_tenant` predicate to qualifying queries
// going through the real DBTools_SQL `IQueryInterceptor` pipeline. The
// interceptor is the single point where the global tenant filter is applied;
// repositories never need to remember to add it themselves.
//
// Design contract (from .specs/1 - modernization-roadmap/design.md):
//   - "every read/write is a lambda" (UC-10) is preserved: the interceptor
//     only appends a parameterized predicate, never rewrites user SQL.
//   - The `Tenants` table itself is a `Platform*` entity and is exempt from
//     the filter; the `Tenants` repository sets `Properties["__bypassTenantFilter"] = true`
//     on the interceptor's `QueryInterceptionContext` for those operations.
//   - SQL injection is structurally impossible: the predicate is a constant
//     string with a parameter; the parameter value is supplied by
//     `ITenantContext.TenantId` and is a `Guid` (not user input).
public sealed class TenantFilterInterceptor : IQueryInterceptor
{
    public const string BypassPropertyKey = "__bypassTenantFilter";
    public const string TenantParameterName = "@ctx_tenant";
    public const string TenantColumn = "tenant_id";

    private readonly Guid? _tenantId;
    private readonly bool _bypass;

    // Captured for inspection by unit tests. The real DBTools library
    // does not surface the post-interceptor SQL on `IAsyncSqlClient`,
    // so the interceptor itself keeps a reference to the last context
    // it saw. Production code ignores this property.
    public QueryInterceptionContext? LastContext { get; private set; }

    public TenantFilterInterceptor(Guid? tenantId, bool bypass = false)
    {
        _tenantId = tenantId;
        _bypass = bypass;
    }

    public void BeforeExecute(QueryInterceptionContext context)
    {
        LastContext = context;

        if (_bypass || _tenantId is null)
        {
            return;
        }

        if (context.Properties.TryGetValue(BypassPropertyKey, out var bypassFlag) && bypassFlag is true)
        {
            return;
        }

        if (context.OperationType == QueryOperationType.Select
            || context.OperationType == QueryOperationType.Update
            || context.OperationType == QueryOperationType.Delete)
        {
            if (context.Sql.Contains(TenantColumn, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            context.Sql = InjectPredicate(context.Sql, "@param" + context.Parameters.Count);
            context.Parameters.Add(_tenantId);
        }
    }

    public void AfterExecute(QueryInterceptionContext context) { }
    public void OnError(QueryInterceptionContext context, System.Exception exception) { }

    private static string InjectPredicate(string sql, string parameterName)
    {
        var trimmed = sql.TrimEnd().TrimEnd(';');
        var upper = trimmed.ToUpperInvariant();
        var whereIdx = upper.IndexOf(" WHERE ", System.StringComparison.Ordinal);
        if (whereIdx >= 0)
        {
            // Insert "tenant_id = @ctx_tenant AND " right after the WHERE keyword.
            var insertAt = whereIdx + " WHERE ".Length;
            return trimmed.Insert(insertAt, TenantColumn + " = " + parameterName + " AND ");
        }
        return trimmed + " WHERE " + TenantColumn + " = " + parameterName;
    }
}

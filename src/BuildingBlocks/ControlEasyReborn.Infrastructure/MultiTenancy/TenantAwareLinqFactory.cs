using ControlEasyReborn.SharedKernel.MultiTenancy;

namespace ControlEasyReborn.Infrastructure.MultiTenancy;

// Per-scope factory that produces a real DBTools `IAsyncSqlClient` with
// the `TenantFilterInterceptor` wired in for the lifetime of the scope.
//
// The real DBTools `ServiceCollectionExtensions.AddDbTools(...)` registers
// `IAsyncSqlClient` as scoped but the interceptors are read from the
// singleton `DbToolsOptions` at construction time. For per-request tenant
// isolation the `AsyncSqlClient` must be constructed per scope with the
// interceptor already attached. This factory exposes a `Create(...)` that
// does exactly that.
//
// The composition root (wave 1.4 `Host/Program.cs`) is expected to
// register a `Func<ITenantContext, IAsyncSqlClient>` or an
// `IServiceScopeFactory` so the Tenants module can resolve a per-request
// `IAsyncSqlClient`. For now, the factory exposes the building blocks
// and the `TenantRepository` consumes them via DI.
public sealed class TenantAwareLinqFactory
{
    private readonly DBTools.Abstractions.ISqlQueryBuilder _queryBuilder;
    private readonly DBTools.Abstractions.ISqlValidator _validator;
    private readonly DBTools.Abstractions.IDbProvider _provider;
    private readonly DBTools.Abstractions.IDbConfiguration _config;

    public TenantAwareLinqFactory(
        DBTools.Abstractions.ISqlQueryBuilder queryBuilder,
        DBTools.Abstractions.ISqlValidator validator,
        DBTools.Abstractions.IDbProvider provider,
        DBTools.Abstractions.IDbConfiguration config)
    {
        _queryBuilder = queryBuilder;
        _validator = validator;
        _provider = provider;
        _config = config;
    }

    public DBTools.Abstractions.IAsyncSqlClient Create(ITenantContext ctx, bool bypassTenantFilter = false)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var client = new DBTools.Core.AsyncSqlClient(_config, _validator, _queryBuilder, _provider);
        client.AddInterceptor(new TenantFilterInterceptor(ctx.TenantId, bypassTenantFilter));
        return client;
    }
}

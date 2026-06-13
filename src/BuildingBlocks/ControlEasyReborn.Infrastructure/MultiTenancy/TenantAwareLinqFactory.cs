using ControlEasyReborn.SharedKernel.MultiTenancy;

namespace ControlEasyReborn.Infrastructure.MultiTenancy;

public interface ITenantAwareLinqFactory
{
    DBTools.Abstractions.IAsyncSqlClient Create(ITenantContext ctx, bool bypassTenantFilter = false);
}

public sealed class TenantAwareLinqFactory : ITenantAwareLinqFactory
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

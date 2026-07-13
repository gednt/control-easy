using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ControlEasyReborn.Api.Hosting;

public sealed class DbToolsHealthCheck : IHealthCheck
{
    private readonly ITenantAwareLinqFactory _factory;

    public DbToolsHealthCheck(ITenantAwareLinqFactory factory)
    {
        _factory = factory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _factory.Create(NullTenantContext.Instance, bypassTenantFilter: true);
            await db.SelectAsync(
                fields: "Id",
                table: "Tenants",
                whereClause: "1 = 0",
                parameters: Array.Empty<object>(),
                ct: cancellationToken);

            return string.IsNullOrWhiteSpace(db.Error)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(db.Error);
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Database query failed.", exception);
        }
    }
}

using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Photos.Infrastructure.Persistence;

public sealed class ConsentPolicyEvaluator : IConsentPolicyEvaluator
{
    private const string TableName = "TenantConsentPolicies";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public ConsentPolicyEvaluator(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<ConsentOutcome> EvaluateAsync(string subjectType, Guid subjectId, string action, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "SubjectCategory, PhotoRequired",
            table: TableName,
            whereClause: "SubjectCategory = @param0",
            parameters: new object[] { subjectType },
            ct: ct);

        if (rows is null || rows.Rows.Count == 0)
        {
            return ConsentOutcome.Permitted;
        }

        var photoRequired = Convert.ToInt32(rows.Rows[0]["PhotoRequired"]) != 0;
        if (photoRequired && action == "access")
        {
            return ConsentOutcome.RequiresAction;
        }
        return ConsentOutcome.Permitted;
    }
}

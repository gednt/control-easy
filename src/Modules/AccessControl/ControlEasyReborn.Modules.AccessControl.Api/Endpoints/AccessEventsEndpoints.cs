using ControlEasyReborn.Modules.AccessControl.Application.Commands;
using ControlEasyReborn.Modules.AccessControl.Application.Contracts;
using ControlEasyReborn.Modules.AccessControl.Application.Handlers;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.AccessControl.Api.Endpoints;

public static class AccessEventsEndpoints
{
    public static IEndpointRouteBuilder MapAccessEventsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/access-events")
            .RequireAuthorization()
            .WithTags("Access Events");

        group.MapPost("/scans", async (
            [FromBody] RecordAccessScanRequest request,
            ITenantContext tenantContext,
            RecordAccessScanHandler handler,
            IValidator<RecordAccessScanCommand> validator,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var profileId = tenantContext.ProfileId ?? Guid.Empty;

            var validation = await validator.ValidateAsync(new RecordAccessScanCommand(
                TenantId: tenantId,
                QrPayload: request.QrPayload,
                Direction: ParseDirection(request.Direction),
                ScanAttemptId: request.ScanAttemptId,
                PerformedByProfileId: profileId,
                GatehouseId: request.GatehouseId,
                ConfirmDuplicate: request.ConfirmDuplicate), ct);

            if (!validation.IsValid)
            {
                throw new Application.Errors.ValidationException(validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
            }

            var cmd = new RecordAccessScanCommand(
                TenantId: tenantId,
                QrPayload: request.QrPayload,
                Direction: ParseDirection(request.Direction),
                ScanAttemptId: request.ScanAttemptId,
                PerformedByProfileId: profileId,
                GatehouseId: request.GatehouseId,
                ConfirmDuplicate: request.ConfirmDuplicate);

            var result = await handler.HandleAsync(cmd, ct);

            if (result.Decision == ScanDecisionKind.Recorded)
            {
                return Results.Ok(new ScanResponse(
                    Decision: ScanDecisionKindCodes.ToWire(result.Decision),
                    AccessEventId: result.AccessEventId!.Value,
                    SubjectType: SubjectTypeCodes.ToWire(result.SubjectType),
                    SubjectId: result.SubjectId,
                    CredentialId: result.CredentialId,
                    LookupAuditId: null,
                    AccessMethod: AccessMethodCodes.ToWire(result.AccessMethod),
                    Direction: CycleDirectionCodes.ToWire(result.Direction),
                    PolicyOutcome: PolicyOutcomeCodes.ToWire(result.PolicyOutcome),
                    DestinationApartmentId: result.DestinationApartmentId!.Value,
                    DestinationBlock: result.DestinationBlock,
                    DestinationUnit: result.DestinationUnit));
            }

            var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Access scan rejected",
                Type = "https://httpstatuses.io/422",
                Detail = result.FailureCode ?? result.Decision.ToString()
            };
            problem.Extensions["failureCode"] = result.FailureCode;
            problem.Extensions["decision"] = ScanDecisionKindCodes.ToWire(result.Decision);
            return Results.Json(problem, statusCode: StatusCodes.Status422UnprocessableEntity);
        })
        .RequireAuthorization("Permission_Access.Access.Operate");

        return app;
    }

    private static CycleDirection ParseDirection(string? value) => value?.ToLowerInvariant() switch
    {
        "exit" => CycleDirection.Exit,
        _ => CycleDirection.Entrance
    };
}
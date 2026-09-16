using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.AccessControl.Api.Endpoints;

public static class AccessCredentialsEndpoints
{
    public static IEndpointRouteBuilder MapAccessCredentialsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/access-credentials")
            .RequireAuthorization()
            .WithTags("Access Credentials");

        group.MapGet("/{id:guid}", async (
            Guid id,
            ITenantContext tenantContext,
            IAccessCredentialRepository credentials,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var credential = await credentials.FindByIdAsync(tenantId, id, ct);
            if (credential is null) return Results.NotFound();
            return Results.Ok(new
            {
                id = credential.Id,
                tenantId = credential.TenantId,
                subjectType = Domain.ValueObjects.SubjectTypeCodes.ToWire(credential.SubjectType),
                subjectId = credential.SubjectId,
                method = Domain.ValueObjects.CredentialMethodCodes.ToWire(credential.Method),
                status = Domain.ValueObjects.CredentialStatusCodes.ToWire(credential.Status),
                validFromUtc = credential.ValidFromUtc,
                expiresAtUtc = credential.ExpiresAtUtc,
                replacedByCredentialId = credential.ReplacedByCredentialId,
                issuedByProfileId = credential.IssuedByProfileId,
                createdAtUtc = credential.CreatedAtUtc,
                updatedAtUtc = credential.UpdatedAtUtc
            });
        })
        .RequireAuthorization("Permission_Access.Read");

        return app;
    }
}
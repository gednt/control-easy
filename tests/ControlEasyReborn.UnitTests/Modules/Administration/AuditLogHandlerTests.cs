using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Errors;
using ControlEasyReborn.Modules.Administration.Application.Handlers;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.Modules.Administration.Infrastructure.Services;
using ControlEasyReborn.SharedKernel.Auditing;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Administration;

public sealed class AuditLogHandlerTests
{
    [Fact]
    public async Task ListAuditLogHandler_should_call_repository_with_parsed_severity_and_return_responses()
    {
        var repo = Substitute.For<IAuditLogRepository>();
        var tenantId = Guid.NewGuid();
        var entry = new AuditLogEntry(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            action: "OverrideAuthorized",
            entityType: "GatehouseEntry",
            entityId: Guid.NewGuid(),
            performedByUserId: Guid.NewGuid(),
            performedByName: "Attendant John",
            details: "Override allowed for visitor",
            createdAtUtc: DateTime.UtcNow,
            category: AuditCategory.Gatehouse,
            severity: AuditSeverity.SecurityAlert,
            metadataJson: "{\"reason\":\"emergency\"}");

        repo.ListAsync(
            tenantId: tenantId,
            category: AuditCategory.Gatehouse,
            severity: AuditSeverity.SecurityAlert,
            entityType: null,
            action: null,
            fromUtc: null,
            toUtc: null,
            searchTerm: null,
            skip: 0,
            take: 50,
            ct: Arg.Any<CancellationToken>())
            .Returns(new List<AuditLogEntry> { entry });

        var handler = new ListAuditLogHandler(repo);

        var result = await handler.HandleAsync(
            tenantId: tenantId,
            category: AuditCategory.Gatehouse,
            severity: "SecurityAlert",
            entityType: null,
            action: null,
            fromUtc: null,
            toUtc: null,
            searchTerm: null,
            skip: 0,
            take: 50,
            ct: CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Category.Should().Be(AuditCategory.Gatehouse);
        result[0].Severity.Should().Be("SecurityAlert");
        result[0].Action.Should().Be("OverrideAuthorized");
        result[0].MetadataJson.Should().Be("{\"reason\":\"emergency\"}");
    }

    [Fact]
    public async Task GetAuditLogByIdHandler_should_return_entry_when_found()
    {
        var repo = Substitute.For<IAuditLogRepository>();
        var id = Guid.NewGuid();
        var entry = new AuditLogEntry(
            id: id,
            tenantId: Guid.NewGuid(),
            action: "EntryRefused",
            entityType: "GatehouseEntry",
            entityId: Guid.NewGuid(),
            performedByUserId: Guid.NewGuid(),
            performedByName: "Officer Bob",
            details: "Consent refused",
            createdAtUtc: DateTime.UtcNow,
            category: AuditCategory.Gatehouse,
            severity: AuditSeverity.Warning);

        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entry);

        var handler = new GetAuditLogByIdHandler(repo);

        var result = await handler.HandleAsync(id, CancellationToken.None);

        result.Id.Should().Be(id);
        result.Action.Should().Be("EntryRefused");
        result.Severity.Should().Be("Warning");
    }

    [Fact]
    public async Task GetAuditLogByIdHandler_should_throw_not_found_when_entry_does_not_exist()
    {
        var repo = Substitute.For<IAuditLogRepository>();
        var id = Guid.NewGuid();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((AuditLogEntry?)null);

        var handler = new GetAuditLogByIdHandler(repo);

        var act = () => handler.HandleAsync(id, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AuditLogWriter_should_format_and_save_audit_entry()
    {
        var repo = Substitute.For<IAuditLogRepository>();
        var writer = new AuditLogWriter(repo);
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        await writer.WriteAsync(
            tenantId: tenantId,
            category: AuditCategory.Visits,
            action: "VisitStarted",
            entityType: "Visit",
            entityId: entityId,
            severity: AuditSeverity.Info,
            details: "Visitor checked in",
            metadata: new { unit = "101", visitor = "Jane" });

        await repo.Received(1).AddAsync(
            Arg.Is<AuditLogEntry>(e =>
                e.TenantId == tenantId &&
                e.Category == AuditCategory.Visits &&
                e.Action == "VisitStarted" &&
                e.Severity == AuditSeverity.Info &&
                e.MetadataJson != null && e.MetadataJson.Contains("101")),
            Arg.Any<CancellationToken>());
    }
}

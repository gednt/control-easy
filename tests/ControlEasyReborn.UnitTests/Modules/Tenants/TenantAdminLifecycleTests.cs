using System.Data;
using ControlEasyReborn.Modules.Tenants.Application;
using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Handlers;
using ControlEasyReborn.Modules.Tenants.Domain;
using ControlEasyReborn.Modules.Tenants.Infrastructure.Persistence;
using ControlEasyReborn.UnitTests.TestDoubles;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Errors = ControlEasyReborn.Modules.Tenants.Application.Errors;

namespace ControlEasyReborn.UnitTests.Modules.Tenants;

public sealed class TenantAdminLifecycleTests
{
    [Fact]
    public async Task SetAdminActiveAsync_suspend_updates_user_deactivates_profiles_revokes_tokens()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new TenantAdminRepository(client);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        client.SelectResultFactory = () => UserRow(userId, tenantId, "TenantAdmin");

        await sut.SetAdminActiveAsync(tenantId, userId, active: false, CancellationToken.None);

        var updates = client.Operations.Where(o => o.OperationType == "Update").ToList();
        updates.Should().HaveCount(3);

        updates[0].Sql.Should().Contain("UPDATE Users");
        updates[0].Sql.Should().Contain("Active=@param0");
        updates[0].Parameters[0].Should().Be("0");

        updates[1].Sql.Should().Contain("UPDATE AttendantProfiles");
        updates[1].Parameters[0].Should().Be("0");
        updates[1].Parameters.Last().Should().Be(userId.ToString());

        updates[2].Sql.Should().Contain("UPDATE RefreshTokens");
        updates[2].Parameters[0].Should().NotBeNull();
        updates[2].Parameters.Last().Should().Be(userId.ToString());
    }

    [Fact]
    public async Task SetAdminActiveAsync_resume_only_sets_active()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new TenantAdminRepository(client);
        client.SelectResultFactory = () => UserRow(Guid.NewGuid(), Guid.NewGuid(), "TenantAdmin");

        await sut.SetAdminActiveAsync(Guid.NewGuid(), Guid.NewGuid(), active: true, CancellationToken.None);

        client.Operations.Where(o => o.OperationType == "Update").Should().HaveCount(1);
        client.Operations.Should().NotContain(o => o.OperationType == "Delete");
    }

    [Fact]
    public async Task SetAdminActiveAsync_unknown_user_throws()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new TenantAdminRepository(client);
        client.SelectResultFactory = () => new DataTable();

        var act = () => sut.SetAdminActiveAsync(Guid.NewGuid(), Guid.NewGuid(), active: false, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAdminAsync_deletes_profiles_tokens_and_user()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new TenantAdminRepository(client);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        client.SelectResultFactory = () => UserRow(userId, tenantId, "TenantAdmin");

        await sut.DeleteAdminAsync(tenantId, userId, CancellationToken.None);

        var deletes = client.Operations.Where(o => o.OperationType == "Delete").ToList();
        deletes.Should().HaveCount(3);
        deletes[0].Sql.Should().Contain("DELETE FROM AttendantProfiles");
        deletes[0].Parameters.Should().Contain(userId.ToString());
        deletes[0].Parameters.Should().Contain(tenantId.ToString());
        deletes[1].Sql.Should().Contain("DELETE FROM RefreshTokens");
        deletes[2].Sql.Should().Contain("DELETE FROM Users");
    }

    [Fact]
    public async Task UpdatePorteiroAsync_updates_user_email_and_profile_display_name()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new TenantAdminRepository(client);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        client.SelectResultFactory = () => UserRow(userId, tenantId, PorteiroDefaults.Role);

        await sut.UpdatePorteiroAsync(tenantId, userId, "new@condo.test", "New Name", CancellationToken.None);

        var updates = client.Operations.Where(o => o.OperationType == "Update").ToList();
        updates.Should().HaveCount(2);
        updates[0].Sql.Should().Contain("UPDATE Users");
        updates[0].Parameters[0].Should().Be("new@condo.test");
        updates[1].Sql.Should().Contain("UPDATE AttendantProfiles");
        updates[1].Parameters[0].Should().Be("New Name");
    }

    [Fact]
    public async Task DeletePorteiroAsync_deletes_profiles_tokens_and_user()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new TenantAdminRepository(client);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        client.SelectResultFactory = () => UserRow(userId, tenantId, PorteiroDefaults.Role);

        await sut.DeletePorteiroAsync(tenantId, userId, CancellationToken.None);

        var deletes = client.Operations.Where(o => o.OperationType == "Delete").ToList();
        deletes.Select(d => d.Sql).Should().Contain(
            "DELETE FROM AttendantProfiles WHERE UserId = @param0 AND TenantId = @param1",
            "DELETE FROM RefreshTokens WHERE UserId = @param0",
            "DELETE FROM Users WHERE Id = @param0");
    }

    [Fact]
    public async Task RevokeAdminAsync_deactivates_profiles_and_revokes_tokens()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new TenantAdminRepository(client);

        await sut.RevokeAdminAsync(Guid.NewGuid(), CancellationToken.None);

        var updates = client.Operations.Where(o => o.OperationType == "Update").ToList();
        updates.Should().HaveCount(3);
        updates[0].Sql.Should().Contain("UPDATE Users");
        updates[1].Sql.Should().Contain("UPDATE AttendantProfiles");
        updates[2].Sql.Should().Contain("UPDATE RefreshTokens");
    }

    [Fact]
    public async Task DeleteTenantAdminHandler_blocks_deleting_last_active_admin()
    {
        var adminRepo = Substitute.For<ITenantAdminRepository>();
        var tenants = Substitute.For<ITenantRepository>();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        tenants.FindAsync(tenantId, Arg.Any<CancellationToken>()).Returns(new Tenant(
            tenantId, "aurora", "Aurora", TenantStatus.Active, DateTime.UtcNow));
        adminRepo.ListAdminsAsync(tenantId, Arg.Any<CancellationToken>()).Returns(
        [
            new ControlEasyReborn.Modules.Tenants.Application.Contracts.TenantAdminResponse(userId, "only@condo.test", "Only Admin", tenantId, true, DateTime.UtcNow),
        ]);

        var sut = new DeleteTenantAdminHandler(adminRepo, tenants);

        var act = () => sut.HandleAsync(tenantId, userId, CancellationToken.None);

        await act.Should().ThrowAsync<Errors.ConflictException>()
            .WithMessage("*last administrator*");
        await adminRepo.DidNotReceive().DeleteAdminAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteTenantAdminHandler_allows_delete_when_another_active_admin_remains()
    {
        var adminRepo = Substitute.For<ITenantAdminRepository>();
        var tenants = Substitute.For<ITenantRepository>();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        tenants.FindAsync(tenantId, Arg.Any<CancellationToken>()).Returns(new Tenant(
            tenantId, "aurora", "Aurora", TenantStatus.Active, DateTime.UtcNow));
        adminRepo.ListAdminsAsync(tenantId, Arg.Any<CancellationToken>()).Returns(
        [
            new ControlEasyReborn.Modules.Tenants.Application.Contracts.TenantAdminResponse(userId, "a@condo.test", "A", tenantId, true, DateTime.UtcNow),
            new ControlEasyReborn.Modules.Tenants.Application.Contracts.TenantAdminResponse(otherId, "b@condo.test", "B", tenantId, true, DateTime.UtcNow),
        ]);

        var sut = new DeleteTenantAdminHandler(adminRepo, tenants);

        await sut.HandleAsync(tenantId, userId, CancellationToken.None);

        await adminRepo.Received(1).DeleteAdminAsync(tenantId, userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteTenantAdminHandler_unknown_user_throws_not_found()
    {
        var adminRepo = Substitute.For<ITenantAdminRepository>();
        var tenants = Substitute.For<ITenantRepository>();
        var tenantId = Guid.NewGuid();
        tenants.FindAsync(tenantId, Arg.Any<CancellationToken>()).Returns(new Tenant(
            tenantId, "aurora", "Aurora", TenantStatus.Active, DateTime.UtcNow));
        adminRepo.ListAdminsAsync(tenantId, Arg.Any<CancellationToken>()).Returns([]);

        var sut = new DeleteTenantAdminHandler(adminRepo, tenants);

        var act = () => sut.HandleAsync(tenantId, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<Errors.NotFoundException>();
    }

    [Fact]
    public async Task RevokeTenantAdminHandler_unknown_user_for_tenant_throws_not_found()
    {
        var adminRepo = Substitute.For<ITenantAdminRepository>();
        var tenants = Substitute.For<ITenantRepository>();
        var tenantId = Guid.NewGuid();
        tenants.FindAsync(tenantId, Arg.Any<CancellationToken>()).Returns(new Tenant(
            tenantId, "aurora", "Aurora", TenantStatus.Active, DateTime.UtcNow));
        adminRepo.ListAdminsAsync(tenantId, Arg.Any<CancellationToken>()).Returns([]);

        var sut = new RevokeTenantAdminHandler(adminRepo, tenants);

        var act = () => sut.HandleAsync(tenantId, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<Errors.NotFoundException>();
        await adminRepo.DidNotReceive().RevokeAdminAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTenantPorteiroHandler_rejects_existing_email()
    {
        var adminRepo = Substitute.For<ITenantAdminRepository>();
        var tenants = Substitute.For<ITenantRepository>();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        tenants.FindAsync(tenantId, Arg.Any<CancellationToken>()).Returns(new Tenant(
            tenantId, "aurora", "Aurora", TenantStatus.Active, DateTime.UtcNow));
        adminRepo.ListPorteirosAsync(tenantId, Arg.Any<CancellationToken>()).Returns(
        [
            new ControlEasyReborn.Modules.Tenants.Application.Contracts.PorteiroResponse(userId, Guid.NewGuid(), "p@condo.test", "P", tenantId, true, DateTime.UtcNow),
        ]);
        adminRepo.EmailExistsAsync("taken@condo.test", userId, Arg.Any<CancellationToken>()).Returns(true);

        var sut = new UpdateTenantPorteiroHandler(adminRepo, tenants);

        var act = () => sut.HandleAsync(tenantId, userId,
            new ControlEasyReborn.Modules.Tenants.Application.Contracts.UpdatePorteiroRequest("taken@condo.test", "P"), CancellationToken.None);

        await act.Should().ThrowAsync<Errors.ConflictException>();
    }

    [Fact]
    public async Task UpdateTenantPorteiroHandler_updates_and_returns_new_details()
    {
        var adminRepo = Substitute.For<ITenantAdminRepository>();
        var tenants = Substitute.For<ITenantRepository>();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        tenants.FindAsync(tenantId, Arg.Any<CancellationToken>()).Returns(new Tenant(
            tenantId, "aurora", "Aurora", TenantStatus.Active, DateTime.UtcNow));
        adminRepo.ListPorteirosAsync(tenantId, Arg.Any<CancellationToken>()).Returns(
        [
            new ControlEasyReborn.Modules.Tenants.Application.Contracts.PorteiroResponse(userId, profileId, "p@condo.test", "P", tenantId, true, DateTime.UtcNow),
        ]);
        adminRepo.EmailExistsAsync("new@condo.test", userId, Arg.Any<CancellationToken>()).Returns(false);

        var sut = new UpdateTenantPorteiroHandler(adminRepo, tenants);

        var response = await sut.HandleAsync(tenantId, userId,
            new ControlEasyReborn.Modules.Tenants.Application.Contracts.UpdatePorteiroRequest("new@condo.test", "New P"), CancellationToken.None);

        response.Email.Should().Be("new@condo.test");
        response.DisplayName.Should().Be("New P");
        await adminRepo.Received(1).UpdatePorteiroAsync(tenantId, userId, "new@condo.test", "New P", Arg.Any<CancellationToken>());
    }

    private static DataTable UserRow(Guid userId, Guid tenantId, string roles)
    {
        var table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("TenantId");
        table.Columns.Add("Email");
        table.Columns.Add("DisplayName");
        table.Columns.Add("Active", typeof(bool));
        table.Columns.Add("Roles");
        table.Columns.Add("CreatedAtUtc", typeof(DateTime));
        table.Rows.Add(userId.ToString(), tenantId.ToString(), "u@condo.test", "U", true, roles, DateTime.UtcNow);
        return table;
    }
}

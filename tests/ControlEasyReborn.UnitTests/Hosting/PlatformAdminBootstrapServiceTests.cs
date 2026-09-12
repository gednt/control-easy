using System.Data;
using ControlEasyReborn.Api.Hosting;
using ControlEasyReborn.Infrastructure.Bootstrap;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.SharedKernel.Bootstrap;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using BootstrapOptions = ControlEasyReborn.SharedKernel.Bootstrap.BootstrapOptions;

namespace ControlEasyReborn.UnitTests.Hosting;

public sealed class PlatformAdminBootstrapServiceTests
{
    private static readonly Guid PlatformTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task StartAsync_when_no_platform_admin_seeds_user_and_attendant_profile()
    {
        var client = new FakeAsyncSqlClient();
        var selectCount = 0;
        client.SelectResultFactory = () =>
        {
            selectCount++;
            return EmptyUsersTable();
        };

        var services = BuildServices(client);
        var sut = services.GetRequiredService<PlatformAdminBootstrapService>();
        var credentialsStore = services.GetRequiredService<IBootstrapCredentialsStore>();

        await sut.StartAsync(CancellationToken.None);

        selectCount.Should().Be(1);
        var inserts = client.Operations.Where(o => o.OperationType == "Insert").ToList();
        inserts.Should().HaveCount(2);
        credentialsStore.TryGet(out var email, out var password).Should().BeTrue();
        email.Should().EndWith("@controleasy.local");
        password.Should().NotBeNullOrWhiteSpace();

        var userInsert = inserts[0];
        userInsert.Sql.Should().Contain("Users");
        userInsert.Parameters.Should().Contain("PlatformAdmin");

        var profileInsert = inserts[1];
        profileInsert.Sql.Should().Contain("AttendantProfiles");
        profileInsert.Parameters.Should().Contain(PlatformTenantId);
        profileInsert.Parameters.Should().Contain("Platform Admin");
        profileInsert.Parameters.Should().Contain("platform:*");
        profileInsert.Parameters.Should().Contain(true);
        profileInsert.Parameters.Should().Contain(userInsert.Parameters[0]);
        profileInsert.Parameters.Should().Contain(DBNull.Value);
    }

    [Fact]
    public async Task StartAsync_when_platform_admin_exists_skips_all_inserts()
    {
        var client = new FakeAsyncSqlClient();
        client.SelectResultFactory = () =>
        {
            var dt = EmptyUsersTable();
            dt.Rows.Add(Guid.NewGuid().ToString());
            return dt;
        };

        var services = BuildServices(client);
        var sut = services.GetRequiredService<PlatformAdminBootstrapService>();

        await sut.StartAsync(CancellationToken.None);

        client.Operations.Where(o => o.OperationType == "Insert").Should().BeEmpty();
    }

    [Fact]
    public async Task StartAsync_when_insert_fails_then_succeeds_retries_and_seeds()
    {
        var client = new FakeAsyncSqlClient();
        var insertCalls = 0;
        client.InsertResultFactory = () => ++insertCalls > 2;

        var services = BuildServices(client);
        var sut = services.GetRequiredService<PlatformAdminBootstrapService>();
        var credentialsStore = services.GetRequiredService<IBootstrapCredentialsStore>();

        await sut.StartAsync(CancellationToken.None);

        insertCalls.Should().Be(4);
        credentialsStore.TryGet(out _, out _).Should().BeTrue();
        client.Operations.Where(o => o.OperationType == "Select").Should().HaveCount(3);
        client.Operations.Where(o => o.OperationType == "Insert" && o.Sql.Contains("Users")).Should().HaveCount(3);
        client.Operations.Where(o => o.OperationType == "Insert" && o.Sql.Contains("AttendantProfiles")).Should().HaveCount(1);
    }

    [Fact]
    public async Task StartAsync_when_insert_never_succeeds_throws()
    {
        var client = new FakeAsyncSqlClient();
        client.InsertResultFactory = () => false;

        var services = BuildServices(client);
        var sut = services.GetRequiredService<PlatformAdminBootstrapService>();

        var act = () => sut.StartAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        var userInserts = client.Operations.Where(o => o.OperationType == "Insert" && o.Sql.Contains("Users")).ToList();
        var profileInserts = client.Operations.Where(o => o.OperationType == "Insert" && o.Sql.Contains("AttendantProfiles")).ToList();
        userInserts.Should().HaveCount(6);
        profileInserts.Should().BeEmpty();
    }

    [Fact]
    public async Task StartAsync_when_select_fails_then_succeeds_retries_before_seeding()
    {
        var client = new FakeAsyncSqlClient();
        client.Error = "connection refused";
        var selectCalls = 0;
        client.SelectResultFactory = () =>
        {
            if (++selectCalls > 2)
                client.Error = string.Empty;
            return EmptyUsersTable();
        };

        var services = BuildServices(client);
        var sut = services.GetRequiredService<PlatformAdminBootstrapService>();
        var credentialsStore = services.GetRequiredService<IBootstrapCredentialsStore>();

        await sut.StartAsync(CancellationToken.None);

        selectCalls.Should().Be(3);
        client.Operations.Where(o => o.OperationType == "Insert").Should().HaveCount(2);
        credentialsStore.TryGet(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_when_select_never_succeeds_throws()
    {
        var client = new FakeAsyncSqlClient();
        client.Error = "Table 'Users' doesn't exist";

        var services = BuildServices(client);
        var sut = services.GetRequiredService<PlatformAdminBootstrapService>();

        var act = () => sut.StartAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        client.Operations.Where(o => o.OperationType == "Insert").Should().BeEmpty();
        client.Operations.Where(o => o.OperationType == "Select").Should().HaveCount(6);
    }

    [Fact]
    public async Task StartAsync_when_user_exists_but_profile_missing_repairs_profile()
    {
        var client = new FakeAsyncSqlClient();
        var selectCalls = 0;
        client.SelectResultFactory = () =>
        {
            selectCalls++;
            if (selectCalls == 1)
            {
                var users = EmptyUsersTable();
                users.Rows.Add("11111111-1111-1111-1111-111111111111");
                return users;
            }
            return EmptyUsersTable();
        };

        var services = BuildServices(client);
        var sut = services.GetRequiredService<PlatformAdminBootstrapService>();

        await sut.StartAsync(CancellationToken.None);

        var inserts = client.Operations.Where(o => o.OperationType == "Insert").ToList();
        inserts.Should().HaveCount(1);
        inserts[0].Sql.Should().Contain("AttendantProfiles");
        inserts[0].Parameters.Should().Contain("11111111-1111-1111-1111-111111111111");
    }

    private static ServiceProvider BuildServices(FakeAsyncSqlClient client)
    {
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");

        var linqFactory = new FakeTenantAwareLinqFactory(client);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITenantAwareLinqFactory>(linqFactory);
        services.AddSingleton(passwordHasher);
        services.AddSingleton<IBootstrapCredentialsStore, BootstrapCredentialsStore>();
        services.Configure<BootstrapOptions>(_ => { });
        services.AddSingleton(sp =>
        {
            var sut = new PlatformAdminBootstrapService(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IBootstrapCredentialsStore>(),
                Microsoft.Extensions.Options.Options.Create(new BootstrapOptions()),
                sp.GetRequiredService<ILogger<PlatformAdminBootstrapService>>());
            sut.DelayAsync = static (_, _) => Task.CompletedTask;
            return sut;
        });
        return services.BuildServiceProvider();
    }

    private static DataTable EmptyUsersTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(string));
        return dt;
    }

    private sealed class FakeTenantAwareLinqFactory(FakeAsyncSqlClient client) : ITenantAwareLinqFactory
    {
        public DBTools.Abstractions.IAsyncSqlClient Create(ITenantContext ctx, bool bypassTenantFilter = false) => client;
    }
}
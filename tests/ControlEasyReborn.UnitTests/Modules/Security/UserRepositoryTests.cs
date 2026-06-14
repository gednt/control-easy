using ControlEasyReborn.Modules.Security.Domain.Entities;
using ControlEasyReborn.Modules.Security.Infrastructure.Persistence;
using ControlEasyReborn.UnitTests.TestDoubles;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Security;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task UpdateAsync_binds_user_id_to_where_parameter_after_set_fields()
    {
        var client = new FakeAsyncSqlClient();
        var sut = new UserRepository(client);
        var userId = Guid.Parse("d8819f8e-1074-49a2-acd6-86446a3cd819");
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var user = new User(userId, tenantId, "platform-admin@controleasy.local", "old-hash", "Platform Admin", true, true, "PlatformAdmin", DateTime.UtcNow);

        user.ChangePassword("new-hash");

        await sut.UpdateAsync(user, CancellationToken.None);

        var update = client.Operations.Single(o => o.OperationType == "Update");
        update.Sql.Should().Contain("WHERE Id = @param8");
        update.Parameters.Should().HaveCount(9);
        update.Parameters[8].Should().Be(userId.ToString());
        update.Parameters[5].Should().Be("0");
    }
}

using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using ControlEasyReborn.Modules.Photos.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using ControlEasyReborn.UnitTests.TestDoubles;
using DBTools.Abstractions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Photos;

public sealed class PhotoRepositoryTests
{
    private readonly Guid _tenantId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private readonly FakeAsyncSqlClient _fakeClient;
    private readonly PhotoRepository _sut;

    public PhotoRepositoryTests()
    {
        _fakeClient = new FakeAsyncSqlClient();
        _fakeClient.AddInterceptor(new TenantFilterInterceptor(_tenantId));
        var factory = new FakeTenantAwareLinqFactory(_fakeClient);
        var ctx = new FakeTenantContext(_tenantId);
        _sut = new PhotoRepository(ctx, factory);
    }

    [Fact]
    public async Task SoftDeleteAsync_UsesParam1ForWhereClauseId()
    {
        var photoId = Guid.NewGuid();
        var photo = new Photo(photoId, _tenantId, "path", null, "image/jpeg", 100, null, DateTime.UtcNow);
        photo.SoftDelete();

        await _sut.SoftDeleteAsync(photo, CancellationToken.None);

        var op = _fakeClient.Operations.Should().ContainSingle().Which;
        op.OperationType.Should().Be("Update");
        op.Sql.Should().Contain("Id = @param1");
        op.Parameters.Should().Contain(photoId);
    }

    private sealed class FakeTenantAwareLinqFactory : ITenantAwareLinqFactory
    {
        private readonly FakeAsyncSqlClient _client;

        public FakeTenantAwareLinqFactory(FakeAsyncSqlClient client) => _client = client;

        public IAsyncSqlClient Create(ITenantContext ctx, bool bypassTenantFilter = false) => _client;
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public FakeTenantContext(Guid tenantId) => TenantId = tenantId;

        public Guid? TenantId { get; }
        public Guid? ProfileId => Guid.Parse("00000000-0000-0000-0000-000000000301");
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
        public bool IsPlatformAdmin => false;
        public bool IsResolved => true;

        public void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions) { }
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Photos.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class PhotoEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;
    private readonly MySqlContainerFixture _mySql;

    public PhotoEndpointTests(MySqlContainerFixture mySql)
    {
        _mySql = mySql;
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task Upload_WhenValidJpegAndPermissionGranted_ReturnsCreatedWithMetadata()
    {
        var client = _factory.AsTenantA();
        var content = new MultipartFormDataContent();
        var fakeJpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
        var fileContent = new ByteArrayContent(fakeJpegBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test_photo.jpg");

        var response = await client.PostAsync("/api/v1/photos", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<PhotoResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.MimeType.Should().Be("image/jpeg");
        body.SizeBytes.Should().Be(fakeJpegBytes.Length);
        body.TenantId.Should().Be(TenantAwareWebApplicationFactory.TenantAId);
        body.FilePath.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Upload_ViaOctetStreamWithImageHeader_ReturnsCreated()
    {
        var client = _factory.AsTenantA();
        var fakePngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var content = new ByteArrayContent(fakePngBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        var response = await client.PostAsync("/api/v1/photos?fileName=test.png", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<PhotoResponse>();
        body.Should().NotBeNull();
        body!.MimeType.Should().Be("image/png");
        body.SizeBytes.Should().Be(fakePngBytes.Length);
    }

    [Fact]
    public async Task Get_WhenPhotoExists_ReturnsBinaryWithCorrectMime()
    {
        var client = _factory.AsTenantA();
        var fakeBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0x12, 0x34 };
        var content = new ByteArrayContent(fakeBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var uploadResponse = await client.PostAsync("/api/v1/photos?fileName=lookup.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await uploadResponse.Content.ReadFromJsonAsync<PhotoResponse>();

        var getResponse = await client.GetAsync($"/api/v1/photos/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/jpeg");

        var downloadedBytes = await getResponse.Content.ReadAsByteArrayAsync();
        downloadedBytes.Should().Equal(fakeBytes);
    }

    [Fact]
    public async Task SoftDelete_WhenPhotoExists_ReturnsNoContent_AndSubsequentGetReturnsNotFound()
    {
        var client = _factory.AsTenantA();
        var fakeBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0xAA, 0xBB };
        var content = new ByteArrayContent(fakeBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var uploadResponse = await client.PostAsync("/api/v1/photos?fileName=delete-me.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await uploadResponse.Content.ReadFromJsonAsync<PhotoResponse>();

        var deleteResponse = await client.DeleteAsync($"/api/v1/photos/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/photos/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrossTenant_Photo_CannotBeAccessedByDifferentTenant()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();

        var fakeBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x99, 0x88 };
        var content = new ByteArrayContent(fakeBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var uploadResponse = await clientA.PostAsync("/api/v1/photos?fileName=tenantA.jpg", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await uploadResponse.Content.ReadFromJsonAsync<PhotoResponse>();

        // Tenant B cannot GET Tenant A's photo
        var getResponse = await clientB.GetAsync($"/api/v1/photos/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Tenant B cannot DELETE Tenant A's photo
        var deleteResponse = await clientB.DeleteAsync($"/api/v1/photos/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_WhenUnsupportedMimeType_Returns415()
    {
        var client = _factory.AsTenantA();
        var content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        var response = await client.PostAsync("/api/v1/photos", content);
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task Upload_WithoutWritePermission_ReturnsForbidden()
    {
        var client = CreateClientWithPermissions(TenantAwareWebApplicationFactory.TenantAId, "Photos.Read");
        var content = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var response = await client.PostAsync("/api/v1/photos", content);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_WithoutReadPermission_ReturnsForbidden()
    {
        var clientA = _factory.AsTenantA();
        var content = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var upload = await clientA.PostAsync("/api/v1/photos", content);
        var created = await upload.Content.ReadFromJsonAsync<PhotoResponse>();

        var unprivilegedClient = CreateClientWithPermissions(TenantAwareWebApplicationFactory.TenantAId, "Photos.Write");
        var response = await unprivilegedClient.GetAsync($"/api/v1/photos/{created!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_WithoutDeletePermission_ReturnsForbidden()
    {
        var clientA = _factory.AsTenantA();
        var content = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var upload = await clientA.PostAsync("/api/v1/photos", content);
        var created = await upload.Content.ReadFromJsonAsync<PhotoResponse>();

        var unprivilegedClient = CreateClientWithPermissions(TenantAwareWebApplicationFactory.TenantAId, "Photos.Read", "Photos.Write");
        var response = await unprivilegedClient.DeleteAsync($"/api/v1/photos/{created!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private HttpClient CreateClientWithPermissions(Guid tenantId, params string[] permissions)
    {
        var client = _factory.CreateClient();
        var token = JwtTestHelper.GenerateTenantToken(
            Guid.NewGuid(),
            "operator@tenanta.test",
            tenantId,
            roles: ["Operator"],
            permissions: permissions);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

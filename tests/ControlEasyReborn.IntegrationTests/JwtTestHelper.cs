using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ControlEasyReborn.IntegrationTests;

public static class JwtTestHelper
{
    public static string Secret = "TestJwtSecretKeyThatIsLongEnoughForHmacSha256Algorithm!2024";
    public static string Issuer = "ControlEasyReborn.Tests";
    public static string Audience = "ControlEasyReborn.Tests";

    public static string GenerateToken(
        Guid userId,
        string email,
        Guid? tenantId = null,
        Guid? profileId = null,
        IReadOnlyCollection<string>? roles = null,
        IReadOnlyCollection<string>? permissions = null,
        TimeSpan? expiration = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (tenantId is not null)
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));

        if (profileId is not null)
            claims.Add(new Claim("profile_id", profileId.Value.ToString()));

        if (roles is not null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("roles", role));
            }
        }

        if (permissions is not null)
        {
            foreach (var perm in permissions)
                claims.Add(new Claim("permissions", perm));
        }

        var expires = expiration ?? TimeSpan.FromHours(1);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expires),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GeneratePlatformAdminToken(
        Guid userId,
        string email,
        TimeSpan? expiration = null)
    {
        return GenerateToken(
            userId,
            email,
            tenantId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            profileId: Guid.Parse("99999999-9999-9999-9999-999999999999"),
            roles: new[] { "PlatformAdmin" },
            permissions: new[] { "platform:*" },
            expiration: expiration);
    }

    public static string GenerateTenantToken(
        Guid userId,
        string email,
        Guid tenantId,
        string[]? roles = null,
        string[]? permissions = null,
        TimeSpan? expiration = null)
    {
        return GenerateToken(
            userId,
            email,
            tenantId: tenantId,
            profileId: Guid.NewGuid(),
            roles: roles,
            permissions: permissions,
            expiration: expiration);
    }
}
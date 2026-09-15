using System.Security.Cryptography;
using System.Text;

namespace ControlEasyReborn.Modules.AccessControl.Infrastructure.Services;

public interface IOpaqueTokenIssuer
{
    IssuedToken Issue(byte[] hmacKey);
    bool Verify(string token, byte[] verifier, int keyVersion, byte[] hmacKey);
    byte[] ComputeVerifier(string token, int keyVersion, byte[] hmacKey);
    byte[] Fingerprint(string token, int keyVersion, byte[] hmacKey);
}

public sealed record IssuedToken(string Token, byte[] Verifier, int KeyVersion, byte[] Fingerprint);

public sealed class OpaqueTokenIssuer : IOpaqueTokenIssuer
{
    public const int TokenByteLength = 32;

    public IssuedToken Issue(byte[] hmacKey)
    {
        if (hmacKey is null || hmacKey.Length == 0)
            throw new ArgumentException("HMAC key is required.", nameof(hmacKey));

        var raw = RandomNumberGenerator.GetBytes(TokenByteLength);
        var encoded = ToBase64Url(raw);
        var keyVersion = 1;
        var verifier = ComputeVerifier(encoded, keyVersion, hmacKey);
        var fingerprint = Fingerprint(encoded, keyVersion, hmacKey);
        return new IssuedToken(encoded, verifier, keyVersion, fingerprint);
    }

    public bool Verify(string token, byte[] verifier, int keyVersion, byte[] hmacKey)
    {
        if (string.IsNullOrEmpty(token)) return false;
        if (verifier is null || verifier.Length == 0) return false;
        var expected = ComputeVerifier(token, keyVersion, hmacKey);
        return CryptographicOperations.FixedTimeEquals(expected, verifier);
    }

    public byte[] ComputeVerifier(string token, int keyVersion, byte[] hmacKey)
    {
        using var hmac = new HMACSHA256(hmacKey);
        var prefix = Encoding.UTF8.GetBytes("v" + keyVersion + ":");
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var combined = new byte[prefix.Length + tokenBytes.Length];
        Buffer.BlockCopy(prefix, 0, combined, 0, prefix.Length);
        Buffer.BlockCopy(tokenBytes, 0, combined, prefix.Length, tokenBytes.Length);
        return hmac.ComputeHash(combined);
    }

    public byte[] Fingerprint(string token, int keyVersion, byte[] hmacKey)
    {
        using var sha = SHA256.Create();
        var prefix = Encoding.UTF8.GetBytes("fp:" + keyVersion + ":");
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var combined = new byte[prefix.Length + tokenBytes.Length];
        Buffer.BlockCopy(prefix, 0, combined, 0, prefix.Length);
        Buffer.BlockCopy(tokenBytes, 0, combined, prefix.Length, tokenBytes.Length);
        return sha.ComputeHash(combined);
    }

    private static string ToBase64Url(byte[] data)
    {
        var b64 = Convert.ToBase64String(data);
        return b64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}

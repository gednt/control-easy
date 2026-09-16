namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface IOpaqueTokenIssuer
{
    IssuedToken Issue(byte[] hmacKey);
    bool Verify(string token, byte[] verifier, int keyVersion, byte[] hmacKey);
    byte[] ComputeVerifier(string token, int keyVersion, byte[] hmacKey);
    byte[] Fingerprint(string token, int keyVersion, byte[] hmacKey);
}

public sealed record IssuedToken(string Token, byte[] Verifier, int KeyVersion, byte[] Fingerprint);
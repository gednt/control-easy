namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public sealed class AccessControlHmacKeyProvider
{
    public byte[] Key { get; }

    public AccessControlHmacKeyProvider()
    {
        var configured = System.Environment.GetEnvironmentVariable("ACCESS_CONTROL_HMAC_KEY");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            Key = System.Text.Encoding.UTF8.GetBytes(configured);
            return;
        }
        Key = System.Text.Encoding.UTF8.GetBytes("CE-DEV-ACCESS-CONTROL-HMAC-KEY-v1-CHANGE-IN-PROD-32B");
    }
}
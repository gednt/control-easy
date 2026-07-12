using ControlEasyReborn.SharedKernel.Bootstrap;

namespace ControlEasyReborn.Infrastructure.Bootstrap;

public sealed class BootstrapCredentialsStore : IBootstrapCredentialsStore
{
    private string? _email;
    private string? _password;

    public void Set(string email, string password)
    {
        _email = email;
        _password = password;
    }

    public bool TryGet(out string email, out string password)
    {
        if (_email is null || _password is null)
        {
            email = string.Empty;
            password = string.Empty;
            return false;
        }

        email = _email;
        password = _password;
        return true;
    }

    public void Clear()
    {
        _email = null;
        _password = null;
    }
}

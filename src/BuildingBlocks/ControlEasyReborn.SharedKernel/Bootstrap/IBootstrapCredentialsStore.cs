namespace ControlEasyReborn.SharedKernel.Bootstrap;

public interface IBootstrapCredentialsStore
{
    void Set(string email, string password);

    bool TryGet(out string email, out string password);

    void Clear();
}

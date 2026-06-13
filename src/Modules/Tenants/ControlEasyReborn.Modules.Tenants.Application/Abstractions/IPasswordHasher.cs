namespace ControlEasyReborn.Modules.Tenants.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
}
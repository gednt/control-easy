using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using BCrypt.Net;

namespace ControlEasyReborn.Modules.Tenants.Infrastructure.Persistence;

public sealed class TenantPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
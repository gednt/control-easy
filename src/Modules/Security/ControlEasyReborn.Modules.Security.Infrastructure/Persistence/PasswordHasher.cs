using ControlEasyReborn.Modules.Security.Application.Abstractions;
using BCrypt.Net;

namespace ControlEasyReborn.Modules.Security.Infrastructure.Persistence;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
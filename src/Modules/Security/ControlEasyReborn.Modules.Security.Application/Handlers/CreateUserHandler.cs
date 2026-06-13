using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class CreateUserHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CreateUserRequest> _validator;

    public CreateUserHandler(IUserRepository users, IPasswordHasher passwordHasher, IValidator<CreateUserRequest> validator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<User> HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var existing = await _users.FindByEmailAsync(request.Email, ct);
        if (existing is not null)
        {
            throw new ConflictException("A user with email '" + request.Email + "' already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(
            id: Guid.NewGuid(),
            tenantId: request.TenantId,
            email: request.Email,
            passwordHash: passwordHash,
            displayName: request.DisplayName,
            active: true,
            mustChangePassword: true,
            roles: request.Roles,
            createdAtUtc: DateTime.UtcNow);

        await _users.AddAsync(user, ct);
        return user;
    }
}
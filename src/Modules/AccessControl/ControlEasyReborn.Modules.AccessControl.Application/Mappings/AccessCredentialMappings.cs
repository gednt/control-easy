using ControlEasyReborn.Modules.AccessControl.Application.Contracts;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using Mapster;

namespace ControlEasyReborn.Modules.AccessControl.Application.Mappings;

public static class AccessCredentialMappings
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AccessCredential, AccessCredentialResponse>()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.TenantId, s => s.TenantId)
            .Map(d => d.SubjectType, s => SubjectTypeCodes.ToWire(s.SubjectType))
            .Map(d => d.SubjectId, s => s.SubjectId)
            .Map(d => d.Method, s => CredentialMethodCodes.ToWire(s.Method))
            .Map(d => d.Status, s => CredentialStatusCodes.ToWire(s.Status))
            .Map(d => d.ValidFromUtc, s => s.ValidFromUtc)
            .Map(d => d.ExpiresAtUtc, s => s.ExpiresAtUtc)
            .Map(d => d.ReplacedByCredentialId, s => s.ReplacedByCredentialId)
            .Map(d => d.IssuedByProfileId, s => s.IssuedByProfileId)
            .Map(d => d.CreatedAtUtc, s => s.CreatedAtUtc)
            .Map(d => d.UpdatedAtUtc, s => s.UpdatedAtUtc)
            .Map(d => d.OneTimeQrPayload, _ => (string?)null);
    }
}
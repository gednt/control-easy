using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Contracts;
using ControlEasyReborn.Modules.Photos.Application.Errors;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Photos.Application.Handlers;

public sealed class UpdateConsentPolicyHandler
{
    private readonly ITenantConsentPolicyRepository _policies;
    private readonly IValidator<UpdateConsentPolicyRequest> _validator;

    public UpdateConsentPolicyHandler(
        ITenantConsentPolicyRepository policies,
        IValidator<UpdateConsentPolicyRequest> validator)
    {
        _policies = policies;
        _validator = validator;
    }

    public async Task<ConsentPolicyResponse> HandleAsync(UpdateConsentPolicyRequest request, Guid tenantId, Guid? profileId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var existing = await _policies.FindByCategoryAsync(tenantId, request.SubjectCategory, ct);
        if (existing is not null)
        {
            existing.Update(request.PhotoRequired, request.DwellTimeLimitMinutes, profileId);
            await _policies.UpsertAsync(existing, ct);
            return ToResponse(existing);
        }

        var policy = new TenantConsentPolicy(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            subjectCategory: request.SubjectCategory,
            photoRequired: request.PhotoRequired,
            dwellTimeLimitMinutes: request.DwellTimeLimitMinutes,
            updatedByProfileId: profileId,
            createdAtUtc: DateTime.UtcNow);

        await _policies.UpsertAsync(policy, ct);
        return ToResponse(policy);
    }

    public async Task<ConsentPolicyResponse?> HandleGetAsync(Guid tenantId, string subjectCategory, CancellationToken ct)
    {
        var policy = await _policies.FindByCategoryAsync(tenantId, subjectCategory, ct);
        return policy is null ? null : ToResponse(policy);
    }

    internal static ConsentPolicyResponse ToResponse(TenantConsentPolicy p) =>
        new(p.Id, p.TenantId, p.SubjectCategory, p.PhotoRequired, p.DwellTimeLimitMinutes, p.UpdatedByProfileId, p.CreatedAtUtc, p.UpdatedAtUtc);
}
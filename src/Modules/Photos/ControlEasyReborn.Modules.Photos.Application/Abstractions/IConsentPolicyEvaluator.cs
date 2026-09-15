namespace ControlEasyReborn.Modules.Photos.Application.Abstractions;

public enum ConsentOutcome
{
    Permitted = 0,
    RequiresAction = 1,
    Refused = 2
}

public interface IConsentPolicyEvaluator
{
    Task<ConsentOutcome> EvaluateAsync(string subjectType, Guid subjectId, string action, CancellationToken ct);
}

using Serilog.Context;

namespace ControlEasyReborn.Modules.AccessControl.Application.Logging;

public static class AccessControlLogContext
{
    public const string TenantIdProperty = "AccessControl.TenantId";
    public const string ProfileIdProperty = "AccessControl.ProfileId";
    public const string GatehouseIdProperty = "AccessControl.GatehouseId";
    public const string ScanAttemptIdProperty = "AccessControl.ScanAttemptId";
    public const string LookupAuditIdProperty = "AccessControl.LookupAuditId";
    public const string DecisionProperty = "AccessControl.Decision";
    public const string CredentialMethodProperty = "AccessControl.CredentialMethod";
    public const string SubjectTypeProperty = "AccessControl.SubjectType";
    public const string DurationMsProperty = "AccessControl.DurationMs";

    private static readonly string[] ReservedSensitivePropertyNames =
    {
        "QrPayload",
        "Qr",
        "Token",
        "RawPayload",
        "Document",
        "DocumentNumber",
        "DocumentValue",
        "Cpf",
        "FullName",
        "Plate"
    };

    public static IDisposable? BeginScope(
        Guid? tenantId = null,
        Guid? profileId = null,
        Guid? gatehouseId = null,
        Guid? scanAttemptId = null,
        Guid? lookupAuditId = null,
        string? decision = null,
        string? credentialMethod = null,
        string? subjectType = null)
    {
        var properties = new List<IDisposable>(capacity: 8);

        if (tenantId.HasValue)
        {
            properties.Add(LogContext.PushProperty(TenantIdProperty, tenantId.Value));
        }
        if (profileId.HasValue)
        {
            properties.Add(LogContext.PushProperty(ProfileIdProperty, profileId.Value));
        }
        if (gatehouseId.HasValue)
        {
            properties.Add(LogContext.PushProperty(GatehouseIdProperty, gatehouseId.Value));
        }
        if (scanAttemptId.HasValue)
        {
            properties.Add(LogContext.PushProperty(ScanAttemptIdProperty, scanAttemptId.Value));
        }
        if (lookupAuditId.HasValue)
        {
            properties.Add(LogContext.PushProperty(LookupAuditIdProperty, lookupAuditId.Value));
        }
        if (!string.IsNullOrWhiteSpace(decision))
        {
            properties.Add(LogContext.PushProperty(DecisionProperty, decision));
        }
        if (!string.IsNullOrWhiteSpace(credentialMethod))
        {
            properties.Add(LogContext.PushProperty(CredentialMethodProperty, credentialMethod));
        }
        if (!string.IsNullOrWhiteSpace(subjectType))
        {
            properties.Add(LogContext.PushProperty(SubjectTypeProperty, subjectType));
        }

        return new CompositeDisposable(properties);
    }

    public static IDisposable PushDuration(long elapsedMilliseconds)
    {
        return LogContext.PushProperty(DurationMsProperty, elapsedMilliseconds);
    }

    public static bool IsSensitivePropertyName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        foreach (var reserved in ReservedSensitivePropertyNames)
        {
            if (string.Equals(name, reserved, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private sealed class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables;
        private bool _disposed;

        public CompositeDisposable(List<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            for (var index = _disposables.Count - 1; index >= 0; index--)
            {
                try
                {
                    _disposables[index].Dispose();
                }
                catch
                {
                }
            }
            _disposables.Clear();
        }
    }
}

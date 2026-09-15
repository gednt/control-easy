namespace ControlEasyReborn.Modules.AccessControl.Domain.Errors;

public sealed class CredentialAlreadyActiveException : Exception
{
    public CredentialAlreadyActiveException(string message) : base(message) { }
}

public sealed class CredentialLifecycleConflictException : Exception
{
    public CredentialLifecycleConflictException(string message) : base(message) { }
}

public sealed class ManualLookupNotFoundException : Exception
{
    public ManualLookupNotFoundException(string message) : base(message) { }
}

public sealed class AccessDestinationRequiredException : Exception
{
    public AccessDestinationRequiredException(string message) : base(message) { }
}

public sealed class AccessDuplicateConfirmationRequiredException : Exception
{
    public AccessDuplicateConfirmationRequiredException(string message) : base(message) { }
}

namespace DeclarationEmployer.Application.Common;

public sealed class ApplicationConflictException : Exception
{
    public ApplicationConflictException(string message)
        : base(message)
    {
    }
}

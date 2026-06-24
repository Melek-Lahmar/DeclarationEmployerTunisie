namespace DeclarationEmployer.Application.Common;

public sealed class ApplicationNotFoundException : Exception
{
    public ApplicationNotFoundException(string message)
        : base(message)
    {
    }
}

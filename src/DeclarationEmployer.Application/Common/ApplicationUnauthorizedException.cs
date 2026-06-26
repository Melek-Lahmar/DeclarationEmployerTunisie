namespace DeclarationEmployer.Application.Common;

public sealed class ApplicationUnauthorizedException : Exception
{
    public ApplicationUnauthorizedException(string message)
        : base(message)
    {
    }
}

namespace DeclarationEmployer.Infrastructure.Configuration;

public sealed class DefaultAdminOptions
{
    public string UserName { get; set; } = "admin";

    public string Email { get; set; } = "admin@local.dev";

    public string Password { get; set; } = "ChangeMe123!";
}

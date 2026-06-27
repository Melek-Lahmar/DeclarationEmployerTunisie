using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class CurrentDeclarationService
{
    public DeclarationDto? Current { get; private set; }

    public bool HasCurrent => Current is not null;

    public event EventHandler? CurrentChanged;

    public void Set(DeclarationDto declaration)
    {
        Current = declaration ?? throw new ArgumentNullException(nameof(declaration));
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        Current = null;
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }
}

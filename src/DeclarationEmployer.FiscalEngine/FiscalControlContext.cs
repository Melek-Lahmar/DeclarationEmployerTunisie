namespace DeclarationEmployer.FiscalEngine;

public sealed class FiscalControlContext
{
    public Guid DeclarationId { get; set; }

    public int FiscalYear { get; set; }

    public IReadOnlyList<FiscalControlLine> Lines { get; set; } = [];
}

namespace DeclarationEmployer.FiscalEngine;

public interface IFiscalControlEngine
{
    FiscalControlResult Run(FiscalControlContext context);
}

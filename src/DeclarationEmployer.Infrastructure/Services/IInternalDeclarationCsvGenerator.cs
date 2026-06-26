using DeclarationEmployer.Domain.Declarations;

namespace DeclarationEmployer.Infrastructure.Services;

public interface IInternalDeclarationCsvGenerator
{
    string Generate(InternalDeclarationCsvDocument document);
}

public sealed class InternalDeclarationCsvDocument
{
    public Guid DeclarationId { get; set; }

    public string ClientCode { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public int FiscalYear { get; set; }

    public IReadOnlyList<InternalDeclarationCsvLine> Lines { get; set; } = [];
}

public sealed class InternalDeclarationCsvLine
{
    public Guid LineId { get; set; }

    public BeneficiaryIdentifierType? BeneficiaryIdentifierType { get; set; }

    public string? BeneficiaryIdentifier { get; set; }

    public string? BeneficiaryName { get; set; }

    public string OperationType { get; set; } = string.Empty;

    public string? FiscalCategory { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal Rate { get; set; }

    public decimal WithheldAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? DocumentReference { get; set; }

    public string? Notes { get; set; }

    public DeclarationLineStatus Status { get; set; }
}

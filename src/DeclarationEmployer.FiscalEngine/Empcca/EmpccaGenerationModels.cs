using DeclarationEmployer.Domain.Declarations.Empcca;

namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed record EmpccaDeclarant(
    string FiscalNumber,
    string FiscalKey,
    string Category,
    string SecondaryEstablishment,
    int Year,
    int ActCode,
    string Name,
    string Activity,
    string City,
    string Street,
    string StreetNumber,
    string PostalCode);

public sealed record EmpccaBeneficiary(
    int IdentifierType,
    string Identifier,
    string Name,
    string ActivityOrJob,
    string Address);

public sealed record EmpccaAnnexA1Record(int OrderNumber, EmpccaBeneficiary Beneficiary, AnnexA1Detail Detail);
public sealed record EmpccaAnnexA2Record(int OrderNumber, EmpccaBeneficiary Beneficiary, AnnexA2Detail Detail);
public sealed record EmpccaAnnexA5Record(int OrderNumber, EmpccaBeneficiary Beneficiary, AnnexA5Detail Detail);

public sealed record EmpccaGenerationArtifact(
    string FileName,
    IReadOnlyList<string> Lines,
    bool IsOfficial,
    IReadOnlyList<string> BlockingIssues)
{
    public string Content => string.Join("\r\n", Lines);
}

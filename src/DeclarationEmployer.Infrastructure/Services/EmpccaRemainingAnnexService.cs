using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations.Empcca;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Domain.Declarations.Empcca;
using DeclarationEmployer.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class EmpccaRemainingAnnexService : DeclarationServiceBase, IEmpccaRemainingAnnexService
{
    private readonly IValidator<CreateEmpccaAnnexA3LineRequest> _a3;
    private readonly IValidator<CreateEmpccaAnnexA4LineRequest> _a4;
    private readonly IValidator<CreateEmpccaAnnexA6LineRequest> _a6;
    private readonly IValidator<CreateEmpccaAnnexA7LineRequest> _a7;

    public EmpccaRemainingAnnexService(ApplicationDbContext db, ICurrentUserService currentUserService, IHostEnvironment environment,
        IValidator<CreateEmpccaAnnexA3LineRequest> a3, IValidator<CreateEmpccaAnnexA4LineRequest> a4,
        IValidator<CreateEmpccaAnnexA6LineRequest> a6, IValidator<CreateEmpccaAnnexA7LineRequest> a7)
        : base(db, currentUserService, environment) => (_a3, _a4, _a6, _a7) = (a3, a4, a6, a7);

    public async Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>>> GetA3LinesAsync(Guid id, CancellationToken ct = default)
    {
        await GetDeclarationAsync(id, ct);
        return (await Query(id, "A3").Include(x => x.AnnexA3Detail).Where(x => x.AnnexA3Detail != null).ToListAsync(ct)).Select(ToA3).ToList();
    }

    public async Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>> CreateA3LineAsync(Guid id, CreateEmpccaAnnexA3LineRequest r, CancellationToken ct = default)
    {
        await Validate(_a3, r, ct); var line = await CreateLine(id, "A3", r.OrderNumber, r.Beneficiary,
            r.SavingsAccountInterest + r.OtherMovableCapitalIncome + r.NonEstablishedBankLoanInterest, r.WithheldAmount, ct);
        line.AnnexA3Detail = new AnnexA3Detail { LineId = line.Id, SavingsAccountInterest = r.SavingsAccountInterest,
            OtherMovableCapitalIncome = r.OtherMovableCapitalIncome, NonEstablishedBankLoanInterest = r.NonEstablishedBankLoanInterest,
            WithheldAmount = r.WithheldAmount, NetPaidAmount = r.NetPaidAmount };
        await Save(id, line, "A3", ct); return ToA3(line);
    }

    public async Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>>> GetA4LinesAsync(Guid id, CancellationToken ct = default)
    {
        await GetDeclarationAsync(id, ct);
        return (await Query(id, "A4").Include(x => x.AnnexA4Detail).Where(x => x.AnnexA4Detail != null).ToListAsync(ct)).Select(ToA4).ToList();
    }

    public async Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>> CreateA4LineAsync(Guid id, CreateEmpccaAnnexA4LineRequest r, CancellationToken ct = default)
    {
        await Validate(_a4, r, ct); var gross = r.ProfessionalAmount + r.ConstructionWorkAmount + r.RealEstateCapitalGainAmount
            + r.SecuritiesCapitalGainAmount + r.SecuritiesIncomeAmount + r.PrivilegedTaxRegimeAmount;
        var line = await CreateLine(id, "A4", r.OrderNumber, r.Beneficiary, gross, r.WithheldAmount, ct);
        line.AnnexA4Detail = new AnnexA4Detail { LineId = line.Id, AmountType = r.AmountType,
            ProfessionalAmountRate = r.ProfessionalAmountRate, ProfessionalAmount = r.ProfessionalAmount,
            ConstructionWorkRate = r.ConstructionWorkRate, ConstructionWorkAmount = r.ConstructionWorkAmount,
            RealEstateCapitalGainRate = r.RealEstateCapitalGainRate, RealEstateCapitalGainAmount = r.RealEstateCapitalGainAmount,
            SecuritiesCapitalGainRate = r.SecuritiesCapitalGainRate, SecuritiesCapitalGainAmount = r.SecuritiesCapitalGainAmount,
            SecuritiesIncomeRate = r.SecuritiesIncomeRate, SecuritiesIncomeAmount = r.SecuritiesIncomeAmount,
            PrivilegedTaxRegimeAmount = r.PrivilegedTaxRegimeAmount, VatWithheldAmount = r.VatWithheldAmount,
            WithheldAmount = r.WithheldAmount, NetPaidAmount = r.NetPaidAmount };
        await Save(id, line, "A4", ct); return ToA4(line);
    }

    public async Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>>> GetA6LinesAsync(Guid id, CancellationToken ct = default)
    {
        await GetDeclarationAsync(id, ct);
        return (await Query(id, "A6").Include(x => x.AnnexA6Detail).Where(x => x.AnnexA6Detail != null).ToListAsync(ct)).Select(ToA6).ToList();
    }

    public async Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>> CreateA6LineAsync(Guid id, CreateEmpccaAnnexA6LineRequest r, CancellationToken ct = default)
    {
        await Validate(_a6, r, ct); var gross = r.RebateAmount + r.FlatRegimeSalesAmount + r.GamblingIncomeAmount
            + r.DistributionNetworkSalesAmount + r.CashCollectionsAmount + r.AlcoholSalesAmount;
        var withheld = r.FlatRegimeSalesAdvanceAmount + r.GamblingWithheldAmount + r.DistributionNetworkWithheldAmount + r.AlcoholSalesAdvanceAmount;
        var line = await CreateLine(id, "A6", r.OrderNumber, r.Beneficiary, gross, withheld, ct);
        line.AnnexA6Detail = new AnnexA6Detail { LineId = line.Id, RebateType = r.RebateType, RebateAmount = r.RebateAmount,
            FlatRegimeSalesAmount = r.FlatRegimeSalesAmount, FlatRegimeSalesAdvanceAmount = r.FlatRegimeSalesAdvanceAmount,
            GamblingIncomeAmount = r.GamblingIncomeAmount, GamblingWithheldAmount = r.GamblingWithheldAmount,
            DistributionNetworkSalesAmount = r.DistributionNetworkSalesAmount, DistributionNetworkWithheldAmount = r.DistributionNetworkWithheldAmount,
            CashCollectionsAmount = r.CashCollectionsAmount, AlcoholSalesAmount = r.AlcoholSalesAmount,
            AlcoholSalesAdvanceAmount = r.AlcoholSalesAdvanceAmount };
        await Save(id, line, "A6", ct); return ToA6(line);
    }

    public async Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>>> GetA7LinesAsync(Guid id, CancellationToken ct = default)
    {
        await GetDeclarationAsync(id, ct);
        return (await Query(id, "A7").Include(x => x.AnnexA7Detail).Where(x => x.AnnexA7Detail != null).ToListAsync(ct)).Select(ToA7).ToList();
    }

    public async Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>> CreateA7LineAsync(Guid id, CreateEmpccaAnnexA7LineRequest r, CancellationToken ct = default)
    {
        await Validate(_a7, r, ct); var line = await CreateLine(id, "A7", r.OrderNumber, r.Beneficiary, r.PaidAmount, r.WithheldAmount, ct);
        line.AnnexA7Detail = new AnnexA7Detail { LineId = line.Id, PaidAmountType = r.PaidAmountType,
            PaidAmount = r.PaidAmount, WithheldAmount = r.WithheldAmount, NetPaidAmount = r.NetPaidAmount };
        await Save(id, line, "A7", ct); return ToA7(line);
    }

    private IQueryable<DeclarationLine> Query(Guid declarationId, string code) => Db.DeclarationLines
        .Include(x => x.Annex).Include(x => x.Beneficiary)
        .Where(x => x.DeclarationId == declarationId && x.Annex != null && x.Annex.AnnexCode == code).OrderBy(x => x.OrderNumber);

    private async Task<DeclarationLine> CreateLine(Guid declarationId, string code, int order, EmpccaBeneficiaryInput input,
        decimal gross, decimal withheld, CancellationToken ct)
    {
        await GetEditableDeclarationAsync(declarationId, ct);
        if (await Query(declarationId, code).AnyAsync(x => x.OrderNumber == order, ct))
            throw new ApplicationConflictException($"Le numero d'ordre {order} existe deja dans {code}.");
        var annex = await Db.DeclarationAnnexes.FirstOrDefaultAsync(x => x.DeclarationId == declarationId && x.AnnexCode == code, ct);
        if (annex is null)
        {
            annex = new DeclarationAnnex { DeclarationId = declarationId, AnnexCode = code, Title = $"Annexe {code[1]} EMPCCA" };
            Db.DeclarationAnnexes.Add(annex);
        }
        var beneficiary = await EnsureBeneficiary(declarationId, input, ct);
        return new DeclarationLine { DeclarationId = declarationId, AnnexId = annex.Id, Annex = annex, BeneficiaryId = beneficiary.Id,
            Beneficiary = beneficiary, OrderNumber = order, OperationType = $"EMPCCA-{code}", GrossAmount = gross,
            TaxableAmount = gross, WithheldAmount = withheld };
    }

    private async Task<DeclarationBeneficiary> EnsureBeneficiary(Guid id, EmpccaBeneficiaryInput input, CancellationToken ct)
    {
        var identifier = input.Identifier.Trim();
        var value = await Db.DeclarationBeneficiaries.FirstOrDefaultAsync(x => x.DeclarationId == id && x.Identifier == identifier, ct);
        if (value is null)
        {
            value = new DeclarationBeneficiary { DeclarationId = id, Identifier = identifier };
            Db.DeclarationBeneficiaries.Add(value);
        }
        value.IdentifierType = input.IdentifierType switch { 1 => BeneficiaryIdentifierType.MatriculeFiscal, 2 => BeneficiaryIdentifierType.CIN,
            3 => BeneficiaryIdentifierType.ResidenceCard, 4 => BeneficiaryIdentifierType.NonDomiciledIdentifier, _ => BeneficiaryIdentifierType.Other };
        value.FullNameOrCompanyName = input.Name.Trim(); value.Activity = input.Activity?.Trim(); value.JobTitle = input.JobTitle?.Trim();
        value.Address = input.Address.Trim(); value.IsResident = input.IdentifierType is 1 or 2; return value;
    }

    private async Task Save(Guid declarationId, DeclarationLine line, string code, CancellationToken ct)
    {
        Db.DeclarationLines.Add(line); AddAudit($"EMPCCA_{code}_LINE_CREATED", nameof(DeclarationLine), line.Id.ToString(), $"Creation ligne {code} detaillee.");
        AddEvent(declarationId, $"EMPCCA_{code}_LINE_CREATED", $"Creation ligne {code} numero {line.OrderNumber}."); await Db.SaveChangesAsync(ct);
    }

    private static async Task Validate<T>(IValidator<T> validator, T request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ApplicationConflictException(string.Join(" ", result.Errors.Select(x => x.ErrorMessage).Distinct()));
    }

    private static EmpccaBeneficiaryInput B(DeclarationLine x) => new() { IdentifierType = x.Beneficiary!.IdentifierType switch
        { BeneficiaryIdentifierType.MatriculeFiscal => 1, BeneficiaryIdentifierType.CIN => 2, BeneficiaryIdentifierType.ResidenceCard => 3,
          BeneficiaryIdentifierType.NonDomiciledIdentifier => 4, _ => 0 }, Identifier = x.Beneficiary.Identifier,
        Name = x.Beneficiary.FullNameOrCompanyName, Activity = x.Beneficiary.Activity, JobTitle = x.Beneficiary.JobTitle,
        Address = x.Beneficiary.Address ?? string.Empty };

    private static EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest> ToA3(DeclarationLine x) { var d = x.AnnexA3Detail!; var b = B(x);
        return new() { Id = x.Id, OrderNumber = x.OrderNumber!.Value, Beneficiary = b, Details = new() { OrderNumber = x.OrderNumber.Value,
            Beneficiary = b, SavingsAccountInterest = d.SavingsAccountInterest, OtherMovableCapitalIncome = d.OtherMovableCapitalIncome,
            NonEstablishedBankLoanInterest = d.NonEstablishedBankLoanInterest, WithheldAmount = d.WithheldAmount, NetPaidAmount = d.NetPaidAmount } }; }

    private static EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest> ToA4(DeclarationLine x) { var d = x.AnnexA4Detail!; var b = B(x);
        return new() { Id = x.Id, OrderNumber = x.OrderNumber!.Value, Beneficiary = b, Details = new() { OrderNumber = x.OrderNumber.Value,
            Beneficiary = b, AmountType = d.AmountType, ProfessionalAmountRate = d.ProfessionalAmountRate, ProfessionalAmount = d.ProfessionalAmount,
            ConstructionWorkRate = d.ConstructionWorkRate, ConstructionWorkAmount = d.ConstructionWorkAmount,
            RealEstateCapitalGainRate = d.RealEstateCapitalGainRate, RealEstateCapitalGainAmount = d.RealEstateCapitalGainAmount,
            SecuritiesCapitalGainRate = d.SecuritiesCapitalGainRate, SecuritiesCapitalGainAmount = d.SecuritiesCapitalGainAmount,
            SecuritiesIncomeRate = d.SecuritiesIncomeRate, SecuritiesIncomeAmount = d.SecuritiesIncomeAmount,
            PrivilegedTaxRegimeAmount = d.PrivilegedTaxRegimeAmount, VatWithheldAmount = d.VatWithheldAmount,
            WithheldAmount = d.WithheldAmount, NetPaidAmount = d.NetPaidAmount } }; }

    private static EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest> ToA6(DeclarationLine x) { var d = x.AnnexA6Detail!; var b = B(x);
        return new() { Id = x.Id, OrderNumber = x.OrderNumber!.Value, Beneficiary = b, Details = new() { OrderNumber = x.OrderNumber.Value,
            Beneficiary = b, RebateType = d.RebateType, RebateAmount = d.RebateAmount, FlatRegimeSalesAmount = d.FlatRegimeSalesAmount,
            FlatRegimeSalesAdvanceAmount = d.FlatRegimeSalesAdvanceAmount, GamblingIncomeAmount = d.GamblingIncomeAmount,
            GamblingWithheldAmount = d.GamblingWithheldAmount, DistributionNetworkSalesAmount = d.DistributionNetworkSalesAmount,
            DistributionNetworkWithheldAmount = d.DistributionNetworkWithheldAmount, CashCollectionsAmount = d.CashCollectionsAmount,
            AlcoholSalesAmount = d.AlcoholSalesAmount, AlcoholSalesAdvanceAmount = d.AlcoholSalesAdvanceAmount } }; }

    private static EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest> ToA7(DeclarationLine x) { var d = x.AnnexA7Detail!; var b = B(x);
        return new() { Id = x.Id, OrderNumber = x.OrderNumber!.Value, Beneficiary = b, Details = new() { OrderNumber = x.OrderNumber.Value,
            Beneficiary = b, PaidAmountType = d.PaidAmountType, PaidAmount = d.PaidAmount,
            WithheldAmount = d.WithheldAmount, NetPaidAmount = d.NetPaidAmount } }; }
}

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

public sealed class EmpccaPriorityAnnexService : DeclarationServiceBase, IEmpccaPriorityAnnexService
{
    private readonly IValidator<CreateEmpccaAnnexA1LineRequest> _a1Validator;
    private readonly IValidator<CreateEmpccaAnnexA2LineRequest> _a2Validator;
    private readonly IValidator<CreateEmpccaAnnexA5LineRequest> _a5Validator;

    public EmpccaPriorityAnnexService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IValidator<CreateEmpccaAnnexA1LineRequest> a1Validator,
        IValidator<CreateEmpccaAnnexA2LineRequest> a2Validator,
        IValidator<CreateEmpccaAnnexA5LineRequest> a5Validator)
        : base(db, currentUserService, environment)
    {
        _a1Validator = a1Validator;
        _a2Validator = a2Validator;
        _a5Validator = a5Validator;
    }

    public async Task<IReadOnlyList<EmpccaAnnexA1LineDto>> GetA1LinesAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);
        var lines = await QueryLines(declarationId, "A1").Include(x => x.AnnexA1Detail)
            .ToListAsync(cancellationToken);
        return lines.Where(x => x.AnnexA1Detail is not null).Select(ToA1Dto).ToList();
    }

    public async Task<EmpccaAnnexA1LineDto> CreateA1LineAsync(
        Guid declarationId,
        CreateEmpccaAnnexA1LineRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_a1Validator, request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        await EnsureOrderIsUniqueAsync(declarationId, "A1", request.OrderNumber, cancellationToken);
        var annex = await EnsureAnnexAsync(declarationId, "A1", "Annexe I - Traitements, salaires, pensions et rentes viageres", cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request.Beneficiary, cancellationToken);
        var withheld = request.CommonRegimeWithheldAmount + request.ForeignEmployeeWithheldAmount + request.SocialSolidarityContribution;
        var line = CreateLine(declarationId, annex.Id, beneficiary.Id, request.OrderNumber, "EMPCCA-A1", request.GrossTaxableIncome, request.TaxableIncome, withheld);
        line.AnnexA1Detail = new AnnexA1Detail
        {
            LineId = line.Id,
            FamilySituation = request.FamilySituation,
            DependentChildrenCount = request.DependentChildrenCount,
            WorkPeriodStart = request.WorkPeriodStart,
            WorkPeriodEnd = request.WorkPeriodEnd,
            WorkPeriodDays = request.WorkPeriodDays,
            TaxableIncome = request.TaxableIncome,
            BenefitsInKind = request.BenefitsInKind,
            GrossTaxableIncome = request.GrossTaxableIncome,
            ReinvestedIncome = request.ReinvestedIncome,
            CommonRegimeWithheldAmount = request.CommonRegimeWithheldAmount,
            ForeignEmployeeWithheldAmount = request.ForeignEmployeeWithheldAmount,
            SocialSolidarityContribution = request.SocialSolidarityContribution,
            NetPaidAmount = request.NetPaidAmount
        };
        Db.DeclarationLines.Add(line);
        AddAudit("EMPCCA_A1_LINE_CREATED", nameof(DeclarationLine), line.Id.ToString(), "Creation ligne detaillee EMPCCA A1.");
        AddEvent(declarationId, "EMPCCA_A1_LINE_CREATED", $"Creation ligne A1 numero {request.OrderNumber}.");
        await Db.SaveChangesAsync(cancellationToken);
        line.Beneficiary = beneficiary;
        return ToA1Dto(line);
    }

    public async Task<EmpccaAnnexA1LineDto> UpdateA1LineAsync(
        Guid declarationId,
        Guid lineId,
        CreateEmpccaAnnexA1LineRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_a1Validator, request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        var line = await GetExistingLineAsync(declarationId, "A1", lineId, cancellationToken);
        await EnsureOrderIsUniqueAsync(declarationId, "A1", request.OrderNumber, lineId, cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request.Beneficiary, cancellationToken);
        var detail = line.AnnexA1Detail ?? new AnnexA1Detail { LineId = line.Id };
        ApplyBaseLine(line, beneficiary.Id, request.OrderNumber, "EMPCCA-A1", request.GrossTaxableIncome, request.TaxableIncome,
            request.CommonRegimeWithheldAmount + request.ForeignEmployeeWithheldAmount + request.SocialSolidarityContribution);
        detail.FamilySituation = request.FamilySituation;
        detail.DependentChildrenCount = request.DependentChildrenCount;
        detail.WorkPeriodStart = request.WorkPeriodStart;
        detail.WorkPeriodEnd = request.WorkPeriodEnd;
        detail.WorkPeriodDays = request.WorkPeriodDays;
        detail.TaxableIncome = request.TaxableIncome;
        detail.BenefitsInKind = request.BenefitsInKind;
        detail.GrossTaxableIncome = request.GrossTaxableIncome;
        detail.ReinvestedIncome = request.ReinvestedIncome;
        detail.CommonRegimeWithheldAmount = request.CommonRegimeWithheldAmount;
        detail.ForeignEmployeeWithheldAmount = request.ForeignEmployeeWithheldAmount;
        detail.SocialSolidarityContribution = request.SocialSolidarityContribution;
        detail.NetPaidAmount = request.NetPaidAmount;
        line.AnnexA1Detail = detail;
        line.Beneficiary = beneficiary;
        await Db.SaveChangesAsync(cancellationToken);
        return ToA1Dto(line);
    }

    public async Task DeleteA1LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        var line = await GetExistingLineAsync(declarationId, "A1", lineId, cancellationToken);
        Db.DeclarationLines.Remove(line);
        AddAudit("EMPCCA_A1_LINE_DELETED", nameof(DeclarationLine), line.Id.ToString(), "Suppression ligne detaillee EMPCCA A1.");
        AddEvent(declarationId, "EMPCCA_A1_LINE_DELETED", $"Suppression ligne A1 numero {line.OrderNumber}.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmpccaAnnexA1SummaryDto> GetA1SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var details = await QueryLines(declarationId, "A1").Where(x => x.AnnexA1Detail != null)
            .Select(x => x.AnnexA1Detail!).ToListAsync(cancellationToken);
        return new EmpccaAnnexA1SummaryDto
        {
            LineCount = details.Count,
            TaxableIncomeTotal = details.Sum(x => x.TaxableIncome),
            GrossTaxableIncomeTotal = details.Sum(x => x.GrossTaxableIncome),
            CommonRegimeWithheldTotal = details.Sum(x => x.CommonRegimeWithheldAmount),
            ForeignEmployeeWithheldTotal = details.Sum(x => x.ForeignEmployeeWithheldAmount),
            SocialSolidarityContributionTotal = details.Sum(x => x.SocialSolidarityContribution),
            NetPaidTotal = details.Sum(x => x.NetPaidAmount)
        };
    }

    public async Task<EmpccaAnnexValidationDto> ValidateA1Async(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var lines = await GetA1LinesAsync(declarationId, cancellationToken);
        var blocking = new List<string>();
        if (lines.Count == 0) blocking.Add("L'annexe A1 ne contient aucune ligne detaillee EMPCCA.");
        if (lines.Select(x => x.OrderNumber).Distinct().Count() != lines.Count) blocking.Add("Les numeros d'ordre A1 doivent etre uniques.");
        if (lines.Any(x => x.Details.WorkPeriodEnd < x.Details.WorkPeriodStart)) blocking.Add("Une periode de travail A1 est invalide.");
        return new EmpccaAnnexValidationDto { BlockingIssues = blocking };
    }

    public async Task<IReadOnlyList<EmpccaAnnexA2LineDto>> GetA2LinesAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);
        var lines = await QueryLines(declarationId, "A2").Include(x => x.AnnexA2Detail)
            .ToListAsync(cancellationToken);
        return lines.Where(x => x.AnnexA2Detail is not null).Select(ToA2Dto).ToList();
    }

    public async Task<EmpccaAnnexA2LineDto> CreateA2LineAsync(Guid declarationId, CreateEmpccaAnnexA2LineRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_a2Validator, request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        await EnsureOrderIsUniqueAsync(declarationId, "A2", request.OrderNumber, cancellationToken);
        var annex = await EnsureAnnexAsync(declarationId, "A2", "Annexe II - Montants servis aux residents", cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request.Beneficiary, cancellationToken);
        var gross = request.GrossProfessionalAmount + request.RealRegimeFeesAmount + request.BoardAndSecuritiesAmount
            + request.OccasionalWorkAmount + request.RealEstateCapitalGainAmount + request.HotelRentAmount + request.ArtistRemunerationAmount;
        var line = CreateLine(declarationId, annex.Id, beneficiary.Id, request.OrderNumber, "EMPCCA-A2", gross, gross, request.WithheldAmount);
        line.AnnexA2Detail = new AnnexA2Detail
        {
            LineId = line.Id,
            AmountType = request.AmountType,
            GrossProfessionalAmount = request.GrossProfessionalAmount,
            RealRegimeFeesAmount = request.RealRegimeFeesAmount,
            BoardAndSecuritiesAmount = request.BoardAndSecuritiesAmount,
            OccasionalWorkAmount = request.OccasionalWorkAmount,
            RealEstateCapitalGainAmount = request.RealEstateCapitalGainAmount,
            HotelRentAmount = request.HotelRentAmount,
            ArtistRemunerationAmount = request.ArtistRemunerationAmount,
            PublicSectorVatWithheldAmount = request.PublicSectorVatWithheldAmount,
            WithheldAmount = request.WithheldAmount,
            NetPaidAmount = request.NetPaidAmount
        };
        Db.DeclarationLines.Add(line);
        AddAudit("EMPCCA_A2_LINE_CREATED", nameof(DeclarationLine), line.Id.ToString(), "Creation ligne detaillee EMPCCA A2.");
        AddEvent(declarationId, "EMPCCA_A2_LINE_CREATED", $"Creation ligne A2 numero {request.OrderNumber}.");
        await Db.SaveChangesAsync(cancellationToken);
        line.Beneficiary = beneficiary;
        return ToA2Dto(line);
    }

    public async Task<EmpccaAnnexA2LineDto> UpdateA2LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA2LineRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_a2Validator, request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        var line = await GetExistingLineAsync(declarationId, "A2", lineId, cancellationToken);
        await EnsureOrderIsUniqueAsync(declarationId, "A2", request.OrderNumber, lineId, cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request.Beneficiary, cancellationToken);
        var gross = request.GrossProfessionalAmount + request.RealRegimeFeesAmount + request.BoardAndSecuritiesAmount
            + request.OccasionalWorkAmount + request.RealEstateCapitalGainAmount + request.HotelRentAmount + request.ArtistRemunerationAmount;
        var detail = line.AnnexA2Detail ?? new AnnexA2Detail { LineId = line.Id };
        ApplyBaseLine(line, beneficiary.Id, request.OrderNumber, "EMPCCA-A2", gross, gross, request.WithheldAmount);
        detail.AmountType = request.AmountType;
        detail.GrossProfessionalAmount = request.GrossProfessionalAmount;
        detail.RealRegimeFeesAmount = request.RealRegimeFeesAmount;
        detail.BoardAndSecuritiesAmount = request.BoardAndSecuritiesAmount;
        detail.OccasionalWorkAmount = request.OccasionalWorkAmount;
        detail.RealEstateCapitalGainAmount = request.RealEstateCapitalGainAmount;
        detail.HotelRentAmount = request.HotelRentAmount;
        detail.ArtistRemunerationAmount = request.ArtistRemunerationAmount;
        detail.PublicSectorVatWithheldAmount = request.PublicSectorVatWithheldAmount;
        detail.WithheldAmount = request.WithheldAmount;
        detail.NetPaidAmount = request.NetPaidAmount;
        line.AnnexA2Detail = detail;
        line.Beneficiary = beneficiary;
        await Db.SaveChangesAsync(cancellationToken);
        return ToA2Dto(line);
    }

    public async Task DeleteA2LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        var line = await GetExistingLineAsync(declarationId, "A2", lineId, cancellationToken);
        Db.DeclarationLines.Remove(line);
        AddAudit("EMPCCA_A2_LINE_DELETED", nameof(DeclarationLine), line.Id.ToString(), "Suppression ligne detaillee EMPCCA A2.");
        AddEvent(declarationId, "EMPCCA_A2_LINE_DELETED", $"Suppression ligne A2 numero {line.OrderNumber}.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmpccaAnnexA2SummaryDto> GetA2SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var details = await QueryLines(declarationId, "A2").Where(x => x.AnnexA2Detail != null)
            .Select(x => x.AnnexA2Detail!).ToListAsync(cancellationToken);
        return new EmpccaAnnexA2SummaryDto
        {
            LineCount = details.Count,
            GrossProfessionalTotal = details.Sum(x => x.GrossProfessionalAmount),
            WithheldTotal = details.Sum(x => x.WithheldAmount),
            NetPaidTotal = details.Sum(x => x.NetPaidAmount)
        };
    }

    public async Task<EmpccaAnnexValidationDto> ValidateA2Async(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var lines = await GetA2LinesAsync(declarationId, cancellationToken);
        var blocking = ValidateOrders(lines.Select(x => x.OrderNumber), "A2");
        if (lines.Any(x => x.Details.AmountType is < 0 or > 6)) blocking.Add("Un type de montant A2 est invalide.");
        return new EmpccaAnnexValidationDto { BlockingIssues = blocking };
    }

    public async Task<IReadOnlyList<EmpccaAnnexA5LineDto>> GetA5LinesAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);
        var lines = await QueryLines(declarationId, "A5").Include(x => x.AnnexA5Detail)
            .ToListAsync(cancellationToken);
        return lines.Where(x => x.AnnexA5Detail is not null).Select(ToA5Dto).ToList();
    }

    public async Task<EmpccaAnnexA5LineDto> CreateA5LineAsync(Guid declarationId, CreateEmpccaAnnexA5LineRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_a5Validator, request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        await EnsureOrderIsUniqueAsync(declarationId, "A5", request.OrderNumber, cancellationToken);
        var annex = await EnsureAnnexAsync(declarationId, "A5", "Annexe V - Autres montants soumis a retenue", cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request.Beneficiary, cancellationToken);
        var gross = request.PurchasesFromTenPercentCompanies + request.PurchasesFromFifteenPercentCompanies
            + request.PurchasesFromTwoThirdsDeductionBusinesses + request.PurchasesFromOtherBusinesses;
        var line = CreateLine(declarationId, annex.Id, beneficiary.Id, request.OrderNumber, "EMPCCA-A5", gross, gross, request.WithheldAmount);
        line.AnnexA5Detail = new AnnexA5Detail
        {
            LineId = line.Id,
            PurchasesFromTenPercentCompanies = request.PurchasesFromTenPercentCompanies,
            PurchasesFromFifteenPercentCompanies = request.PurchasesFromFifteenPercentCompanies,
            PurchasesFromTwoThirdsDeductionBusinesses = request.PurchasesFromTwoThirdsDeductionBusinesses,
            PurchasesFromOtherBusinesses = request.PurchasesFromOtherBusinesses,
            VatWithheldAmount = request.VatWithheldAmount,
            DeliveryPlatformThreePercentWithheldAmount = request.DeliveryPlatformThreePercentWithheldAmount,
            WithheldAmount = request.WithheldAmount,
            NetPaidAmount = request.NetPaidAmount
        };
        Db.DeclarationLines.Add(line);
        AddAudit("EMPCCA_A5_LINE_CREATED", nameof(DeclarationLine), line.Id.ToString(), "Creation ligne detaillee EMPCCA A5.");
        AddEvent(declarationId, "EMPCCA_A5_LINE_CREATED", $"Creation ligne A5 numero {request.OrderNumber}.");
        await Db.SaveChangesAsync(cancellationToken);
        line.Beneficiary = beneficiary;
        return ToA5Dto(line);
    }

    public async Task<EmpccaAnnexA5LineDto> UpdateA5LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA5LineRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_a5Validator, request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        var line = await GetExistingLineAsync(declarationId, "A5", lineId, cancellationToken);
        await EnsureOrderIsUniqueAsync(declarationId, "A5", request.OrderNumber, lineId, cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request.Beneficiary, cancellationToken);
        var gross = request.PurchasesFromTenPercentCompanies + request.PurchasesFromFifteenPercentCompanies
            + request.PurchasesFromTwoThirdsDeductionBusinesses + request.PurchasesFromOtherBusinesses;
        var detail = line.AnnexA5Detail ?? new AnnexA5Detail { LineId = line.Id };
        ApplyBaseLine(line, beneficiary.Id, request.OrderNumber, "EMPCCA-A5", gross, gross, request.WithheldAmount);
        detail.PurchasesFromTenPercentCompanies = request.PurchasesFromTenPercentCompanies;
        detail.PurchasesFromFifteenPercentCompanies = request.PurchasesFromFifteenPercentCompanies;
        detail.PurchasesFromTwoThirdsDeductionBusinesses = request.PurchasesFromTwoThirdsDeductionBusinesses;
        detail.PurchasesFromOtherBusinesses = request.PurchasesFromOtherBusinesses;
        detail.VatWithheldAmount = request.VatWithheldAmount;
        detail.DeliveryPlatformThreePercentWithheldAmount = request.DeliveryPlatformThreePercentWithheldAmount;
        detail.WithheldAmount = request.WithheldAmount;
        detail.NetPaidAmount = request.NetPaidAmount;
        line.AnnexA5Detail = detail;
        line.Beneficiary = beneficiary;
        await Db.SaveChangesAsync(cancellationToken);
        return ToA5Dto(line);
    }

    public async Task DeleteA5LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        var line = await GetExistingLineAsync(declarationId, "A5", lineId, cancellationToken);
        Db.DeclarationLines.Remove(line);
        AddAudit("EMPCCA_A5_LINE_DELETED", nameof(DeclarationLine), line.Id.ToString(), "Suppression ligne detaillee EMPCCA A5.");
        AddEvent(declarationId, "EMPCCA_A5_LINE_DELETED", $"Suppression ligne A5 numero {line.OrderNumber}.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmpccaAnnexA5SummaryDto> GetA5SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var details = await QueryLines(declarationId, "A5").Where(x => x.AnnexA5Detail != null)
            .Select(x => x.AnnexA5Detail!).ToListAsync(cancellationToken);
        return new EmpccaAnnexA5SummaryDto
        {
            LineCount = details.Count,
            PurchasesTotal = details.Sum(x => x.PurchasesFromTenPercentCompanies + x.PurchasesFromFifteenPercentCompanies
                + x.PurchasesFromTwoThirdsDeductionBusinesses + x.PurchasesFromOtherBusinesses),
            VatWithheldTotal = details.Sum(x => x.VatWithheldAmount),
            DeliveryPlatformThreePercentWithheldTotal = details.Sum(x => x.DeliveryPlatformThreePercentWithheldAmount),
            WithheldTotal = details.Sum(x => x.WithheldAmount),
            NetPaidTotal = details.Sum(x => x.NetPaidAmount)
        };
    }

    public async Task<EmpccaAnnexValidationDto> ValidateA5Async(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var lines = await GetA5LinesAsync(declarationId, cancellationToken);
        return new EmpccaAnnexValidationDto { BlockingIssues = ValidateOrders(lines.Select(x => x.OrderNumber), "A5") };
    }

    private IQueryable<DeclarationLine> QueryLines(Guid declarationId, string annexCode) => Db.DeclarationLines
        .Include(x => x.Annex).Include(x => x.Beneficiary)
        .Where(x => x.DeclarationId == declarationId && x.Annex != null && x.Annex.AnnexCode == annexCode)
        .OrderBy(x => x.OrderNumber);

    private async Task EnsureOrderIsUniqueAsync(Guid declarationId, string annexCode, int orderNumber, CancellationToken cancellationToken)
    {
        if (await QueryLines(declarationId, annexCode).AnyAsync(x => x.OrderNumber == orderNumber, cancellationToken))
            throw new ApplicationConflictException($"Le numero d'ordre {orderNumber} existe deja dans {annexCode}.");
    }

    private async Task EnsureOrderIsUniqueAsync(Guid declarationId, string annexCode, int orderNumber, Guid lineId, CancellationToken cancellationToken)
    {
        if (await QueryLines(declarationId, annexCode).AnyAsync(x => x.OrderNumber == orderNumber && x.Id != lineId, cancellationToken))
            throw new ApplicationConflictException($"Le numero d'ordre {orderNumber} existe deja dans {annexCode}.");
    }

    private async Task<DeclarationLine> GetExistingLineAsync(Guid declarationId, string annexCode, Guid lineId, CancellationToken cancellationToken)
    {
        IQueryable<DeclarationLine> query = QueryLines(declarationId, annexCode);
        query = annexCode switch
        {
            "A1" => query.Include(x => x.AnnexA1Detail),
            "A2" => query.Include(x => x.AnnexA2Detail),
            "A5" => query.Include(x => x.AnnexA5Detail),
            _ => query
        };

        var line = await query.FirstOrDefaultAsync(x => x.Id == lineId, cancellationToken);

        if (line is null)
        {
            throw new ApplicationNotFoundException($"Ligne {annexCode} introuvable.");
        }

        return line;
    }

    private async Task<DeclarationAnnex> EnsureAnnexAsync(Guid declarationId, string code, string title, CancellationToken cancellationToken)
    {
        var annex = await Db.DeclarationAnnexes.FirstOrDefaultAsync(
            x => x.DeclarationId == declarationId && x.AnnexCode == code, cancellationToken);
        if (annex is not null) return annex;
        annex = new DeclarationAnnex { Id = Guid.NewGuid(), DeclarationId = declarationId, AnnexCode = code, Title = title };
        Db.DeclarationAnnexes.Add(annex);
        return annex;
    }

    private async Task<DeclarationBeneficiary> EnsureBeneficiaryAsync(Guid declarationId, EmpccaBeneficiaryInput input, CancellationToken cancellationToken)
    {
        var identifier = input.Identifier.Trim();
        var beneficiary = await Db.DeclarationBeneficiaries.FirstOrDefaultAsync(
            x => x.DeclarationId == declarationId && x.Identifier == identifier, cancellationToken);
        if (beneficiary is null)
        {
            beneficiary = new DeclarationBeneficiary { Id = Guid.NewGuid(), DeclarationId = declarationId, Identifier = identifier };
            Db.DeclarationBeneficiaries.Add(beneficiary);
        }
        beneficiary.IdentifierType = input.IdentifierType switch
        {
            1 => BeneficiaryIdentifierType.MatriculeFiscal,
            2 => BeneficiaryIdentifierType.CIN,
            3 => BeneficiaryIdentifierType.ResidenceCard,
            4 => BeneficiaryIdentifierType.NonDomiciledIdentifier,
            _ => BeneficiaryIdentifierType.Other
        };
        beneficiary.FullNameOrCompanyName = input.Name.Trim();
        beneficiary.Activity = input.Activity?.Trim();
        beneficiary.JobTitle = input.JobTitle?.Trim();
        beneficiary.Address = input.Address.Trim();
        beneficiary.IsResident = input.IdentifierType is 1 or 2;
        beneficiary.UpdatedAt = DateTimeOffset.UtcNow;
        return beneficiary;
    }

    private static DeclarationLine CreateLine(Guid declarationId, Guid annexId, Guid beneficiaryId, int orderNumber,
        string operationType, decimal gross, decimal taxable, decimal withheld) => new()
    {
        Id = Guid.NewGuid(), DeclarationId = declarationId, AnnexId = annexId, BeneficiaryId = beneficiaryId,
        OrderNumber = orderNumber, OperationType = operationType, GrossAmount = gross, TaxableAmount = taxable,
        WithheldAmount = withheld, Status = DeclarationLineStatus.Draft, CreatedAt = DateTimeOffset.UtcNow
    };

    private static async Task ValidateAsync<T>(IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new ApplicationConflictException(string.Join(" ", result.Errors.Select(x => x.ErrorMessage).Distinct()));
    }

    private static void ApplyBaseLine(DeclarationLine line, Guid beneficiaryId, int orderNumber, string operationType, decimal gross, decimal taxable, decimal withheld)
    {
        line.BeneficiaryId = beneficiaryId;
        line.OrderNumber = orderNumber;
        line.OperationType = operationType;
        line.GrossAmount = gross;
        line.TaxableAmount = taxable;
        line.WithheldAmount = withheld;
        line.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static List<string> ValidateOrders(IEnumerable<int> values, string annexCode)
    {
        var orders = values.ToList();
        var issues = new List<string>();
        if (orders.Count == 0) issues.Add($"L'annexe {annexCode} ne contient aucune ligne detaillee EMPCCA.");
        if (orders.Distinct().Count() != orders.Count) issues.Add($"Les numeros d'ordre {annexCode} doivent etre uniques.");
        return issues;
    }

    private static EmpccaBeneficiaryInput ToBeneficiary(DeclarationBeneficiary value) => new()
    {
        IdentifierType = value.IdentifierType switch { BeneficiaryIdentifierType.MatriculeFiscal => 1, BeneficiaryIdentifierType.CIN => 2,
            BeneficiaryIdentifierType.ResidenceCard => 3, BeneficiaryIdentifierType.NonDomiciledIdentifier => 4, _ => 0 },
        Identifier = value.Identifier, Name = value.FullNameOrCompanyName, Activity = value.Activity,
        JobTitle = value.JobTitle, Address = value.Address ?? string.Empty
    };

    private static EmpccaAnnexA1LineDto ToA1Dto(DeclarationLine line)
    {
        var d = line.AnnexA1Detail!; var b = ToBeneficiary(line.Beneficiary!);
        var details = new CreateEmpccaAnnexA1LineRequest { OrderNumber = line.OrderNumber!.Value, Beneficiary = b,
            FamilySituation = d.FamilySituation, DependentChildrenCount = d.DependentChildrenCount,
            WorkPeriodStart = d.WorkPeriodStart, WorkPeriodEnd = d.WorkPeriodEnd, WorkPeriodDays = d.WorkPeriodDays,
            TaxableIncome = d.TaxableIncome, BenefitsInKind = d.BenefitsInKind, GrossTaxableIncome = d.GrossTaxableIncome,
            ReinvestedIncome = d.ReinvestedIncome, CommonRegimeWithheldAmount = d.CommonRegimeWithheldAmount,
            ForeignEmployeeWithheldAmount = d.ForeignEmployeeWithheldAmount,
            SocialSolidarityContribution = d.SocialSolidarityContribution, NetPaidAmount = d.NetPaidAmount };
        return new EmpccaAnnexA1LineDto { Id = line.Id, OrderNumber = details.OrderNumber, Beneficiary = b, Details = details };
    }

    private static EmpccaAnnexA2LineDto ToA2Dto(DeclarationLine line)
    {
        var d = line.AnnexA2Detail!; var b = ToBeneficiary(line.Beneficiary!);
        var details = new CreateEmpccaAnnexA2LineRequest { OrderNumber = line.OrderNumber!.Value, Beneficiary = b,
            AmountType = d.AmountType, GrossProfessionalAmount = d.GrossProfessionalAmount,
            RealRegimeFeesAmount = d.RealRegimeFeesAmount, BoardAndSecuritiesAmount = d.BoardAndSecuritiesAmount,
            OccasionalWorkAmount = d.OccasionalWorkAmount, RealEstateCapitalGainAmount = d.RealEstateCapitalGainAmount,
            HotelRentAmount = d.HotelRentAmount, ArtistRemunerationAmount = d.ArtistRemunerationAmount,
            PublicSectorVatWithheldAmount = d.PublicSectorVatWithheldAmount, WithheldAmount = d.WithheldAmount,
            NetPaidAmount = d.NetPaidAmount };
        return new EmpccaAnnexA2LineDto { Id = line.Id, OrderNumber = details.OrderNumber, Beneficiary = b, Details = details };
    }

    private static EmpccaAnnexA5LineDto ToA5Dto(DeclarationLine line)
    {
        var d = line.AnnexA5Detail!; var b = ToBeneficiary(line.Beneficiary!);
        var details = new CreateEmpccaAnnexA5LineRequest { OrderNumber = line.OrderNumber!.Value, Beneficiary = b,
            PurchasesFromTenPercentCompanies = d.PurchasesFromTenPercentCompanies,
            PurchasesFromFifteenPercentCompanies = d.PurchasesFromFifteenPercentCompanies,
            PurchasesFromTwoThirdsDeductionBusinesses = d.PurchasesFromTwoThirdsDeductionBusinesses,
            PurchasesFromOtherBusinesses = d.PurchasesFromOtherBusinesses, VatWithheldAmount = d.VatWithheldAmount,
            DeliveryPlatformThreePercentWithheldAmount = d.DeliveryPlatformThreePercentWithheldAmount,
            WithheldAmount = d.WithheldAmount, NetPaidAmount = d.NetPaidAmount };
        return new EmpccaAnnexA5LineDto { Id = line.Id, OrderNumber = details.OrderNumber, Beneficiary = b, Details = details };
    }
}

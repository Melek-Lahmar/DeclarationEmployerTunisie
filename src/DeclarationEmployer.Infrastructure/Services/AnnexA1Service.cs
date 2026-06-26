using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations.AnnexA1;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class AnnexA1Service : DeclarationServiceBase, IAnnexA1Service
{
    private const string AnnexCode = "A1";
    private const string AnnexTitle = "Annexe I - Traitements, salaires, pensions et rentes viageres";

    public AnnexA1Service(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment)
        : base(db, currentUserService, environment)
    {
    }

    public async Task<IReadOnlyList<AnnexA1LineDto>> GetLinesAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        var lines = await QueryA1Lines(declarationId)
            .ToListAsync(cancellationToken);

        return lines.Select(x => ToDto(x, x.Beneficiary!, x.Annex!)).ToList();
    }

    public async Task<AnnexA1LineDto> CreateLineAsync(
        Guid declarationId,
        CreateAnnexA1LineRequest request,
        CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        ValidateRequest(request);

        var annex = await EnsureAnnexAsync(declarationId, cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request, cancellationToken);

        var line = new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            AnnexId = annex.Id,
            BeneficiaryId = beneficiary.Id,
            OperationType = NormalizeRequired(request.OperationType),
            FiscalCategory = Normalize(request.FiscalCategory),
            GrossAmount = request.GrossAmount,
            TaxableAmount = request.TaxableAmount,
            Rate = request.Rate,
            WithheldAmount = request.WithheldAmount,
            PaymentDate = request.PaymentDate,
            Notes = Normalize(request.Notes),
            Status = DeclarationLineStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationLines.Add(line);
        AddAudit("ANNEX_A1_LINE_CREATED", nameof(DeclarationLine), line.Id.ToString(), "Creation ligne Annexe I foundation.");
        AddEvent(declarationId, "ANNEX_A1_LINE_CREATED", $"Creation ligne Annexe I pour {beneficiary.FullNameOrCompanyName}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(line, beneficiary, annex);
    }

    public async Task DeleteLineAsync(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var line = await QueryA1Lines(declarationId)
            .FirstOrDefaultAsync(x => x.Id == lineId, cancellationToken);

        if (line is null)
        {
            throw new ApplicationNotFoundException("Ligne Annexe I introuvable.");
        }

        Db.DeclarationLines.Remove(line);
        AddAudit("ANNEX_A1_LINE_DELETED", nameof(DeclarationLine), line.Id.ToString(), "Suppression ligne Annexe I foundation.");
        AddEvent(declarationId, "ANNEX_A1_LINE_DELETED", "Suppression ligne Annexe I.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AnnexA1SummaryDto> GetSummaryAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        var lines = await QueryA1Lines(declarationId).ToListAsync(cancellationToken);

        return new AnnexA1SummaryDto
        {
            DeclarationId = declarationId,
            LinesCount = lines.Count,
            BeneficiariesCount = lines
                .Where(x => x.BeneficiaryId.HasValue)
                .Select(x => x.BeneficiaryId!.Value)
                .Distinct()
                .Count(),
            GrossAmountTotal = lines.Sum(x => x.GrossAmount),
            TaxableAmountTotal = lines.Sum(x => x.TaxableAmount),
            WithheldAmountTotal = lines.Sum(x => x.WithheldAmount),
            NetPaidAmountTotal = lines.Sum(x => x.GrossAmount - x.WithheldAmount),
            IsOfficialMappingConfirmed = false,
            MappingMessage = FiscalReferenceSeedService.OfficialMappingIncompleteMessage
        };
    }

    public async Task<AnnexA1LineValidationDto> ValidateLineAsync(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        var line = await QueryA1Lines(declarationId)
            .FirstOrDefaultAsync(x => x.Id == lineId, cancellationToken);

        if (line is null)
        {
            throw new ApplicationNotFoundException("Ligne Annexe I introuvable.");
        }

        var blockingIssues = new List<string>();
        var warnings = new List<string>();

        if (line.Beneficiary is null || string.IsNullOrWhiteSpace(line.Beneficiary.Identifier))
        {
            blockingIssues.Add("Identifiant beneficiaire obligatoire.");
        }

        if (line.Beneficiary is null || string.IsNullOrWhiteSpace(line.Beneficiary.FullNameOrCompanyName))
        {
            blockingIssues.Add("Nom beneficiaire obligatoire.");
        }

        if (line.GrossAmount < 0 || line.TaxableAmount < 0 || line.WithheldAmount < 0)
        {
            blockingIssues.Add("Les montants doivent etre positifs ou nuls.");
        }

        if (line.WithheldAmount > line.TaxableAmount)
        {
            blockingIssues.Add("La retenue ne doit pas depasser le montant imposable.");
        }

        if (line.PaymentDate is null)
        {
            warnings.Add("Date de paiement absente.");
        }

        return new AnnexA1LineValidationDto
        {
            LineId = line.Id,
            IsValid = blockingIssues.Count == 0,
            BlockingIssues = blockingIssues,
            Warnings = warnings
        };
    }

    private IQueryable<DeclarationLine> QueryA1Lines(Guid declarationId)
    {
        return Db.DeclarationLines
            .Include(x => x.Annex)
            .Include(x => x.Beneficiary)
            .Where(x => x.DeclarationId == declarationId
                && x.Annex != null
                && x.Annex.AnnexCode == AnnexCode)
            .OrderByDescending(x => x.CreatedAt);
    }

    private async Task<DeclarationAnnex> EnsureAnnexAsync(Guid declarationId, CancellationToken cancellationToken)
    {
        var annex = await Db.DeclarationAnnexes
            .FirstOrDefaultAsync(x => x.DeclarationId == declarationId && x.AnnexCode == AnnexCode, cancellationToken);

        if (annex is not null)
        {
            return annex;
        }

        annex = new DeclarationAnnex
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            AnnexCode = AnnexCode,
            Title = AnnexTitle,
            Status = DeclarationAnnexStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationAnnexes.Add(annex);
        return annex;
    }

    private async Task<DeclarationBeneficiary> EnsureBeneficiaryAsync(
        Guid declarationId,
        CreateAnnexA1LineRequest request,
        CancellationToken cancellationToken)
    {
        var identifier = NormalizeRequired(request.BeneficiaryIdentifier);
        var identifierType = ParseIdentifierType(request.BeneficiaryIdentifierType);

        var beneficiary = await Db.DeclarationBeneficiaries
            .FirstOrDefaultAsync(x => x.DeclarationId == declarationId && x.Identifier == identifier, cancellationToken);

        if (beneficiary is not null)
        {
            beneficiary.IdentifierType = identifierType;
            beneficiary.FullNameOrCompanyName = NormalizeRequired(request.BeneficiaryName);
            beneficiary.Address = Normalize(request.BeneficiaryAddress);
            beneficiary.UpdatedAt = DateTimeOffset.UtcNow;
            return beneficiary;
        }

        beneficiary = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            IdentifierType = identifierType,
            Identifier = identifier,
            FullNameOrCompanyName = NormalizeRequired(request.BeneficiaryName),
            Address = Normalize(request.BeneficiaryAddress),
            IsResident = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationBeneficiaries.Add(beneficiary);
        return beneficiary;
    }

    private static void ValidateRequest(CreateAnnexA1LineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BeneficiaryIdentifier))
        {
            throw new ApplicationConflictException("Identifiant beneficiaire obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.BeneficiaryName))
        {
            throw new ApplicationConflictException("Nom beneficiaire obligatoire.");
        }

        if (request.GrossAmount < 0 || request.TaxableAmount < 0 || request.WithheldAmount < 0)
        {
            throw new ApplicationConflictException("Les montants doivent etre positifs ou nuls.");
        }

        if (request.WithheldAmount > request.TaxableAmount)
        {
            throw new ApplicationConflictException("La retenue ne doit pas depasser le montant imposable.");
        }

        if (request.Rate is < 0 or > 100)
        {
            throw new ApplicationConflictException("Le taux doit etre compris entre 0 et 100.");
        }
    }

    private static BeneficiaryIdentifierType ParseIdentifierType(string value)
    {
        if (Enum.TryParse<BeneficiaryIdentifierType>(value, true, out var parsed))
        {
            return parsed;
        }

        return BeneficiaryIdentifierType.Other;
    }

    private static string NormalizeRequired(string value) => value.Trim();

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AnnexA1LineDto ToDto(
        DeclarationLine line,
        DeclarationBeneficiary beneficiary,
        DeclarationAnnex annex)
    {
        return new AnnexA1LineDto
        {
            Id = line.Id,
            DeclarationId = line.DeclarationId,
            AnnexId = annex.Id,
            BeneficiaryId = beneficiary.Id,
            BeneficiaryIdentifierType = beneficiary.IdentifierType.ToString(),
            BeneficiaryIdentifier = beneficiary.Identifier,
            BeneficiaryName = beneficiary.FullNameOrCompanyName,
            BeneficiaryAddress = beneficiary.Address,
            OperationType = line.OperationType,
            FiscalCategory = line.FiscalCategory,
            GrossAmount = line.GrossAmount,
            TaxableAmount = line.TaxableAmount,
            Rate = line.Rate,
            WithheldAmount = line.WithheldAmount,
            NetPaidAmount = line.GrossAmount - line.WithheldAmount,
            PaymentDate = line.PaymentDate,
            Notes = line.Notes,
            Status = line.Status.ToString()
        };
    }
}

using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations.AnnexFoundation;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class AnnexFoundationService : DeclarationServiceBase, IAnnexFoundationService
{
    private static readonly HashSet<string> AllowedAnnexCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "A2",
        "A3",
        "A4",
        "A5",
        "A6",
        "A7"
    };

    public AnnexFoundationService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment)
        : base(db, currentUserService, environment)
    {
    }

    public async Task<IReadOnlyList<AnnexFoundationLineDto>> GetLinesAsync(
        Guid declarationId,
        string annexCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeAnnexCode(annexCode);
        await GetDeclarationAsync(declarationId, cancellationToken);

        var lines = await QueryLines(declarationId, normalizedCode)
            .ToListAsync(cancellationToken);

        return lines.Select(x => ToDto(x, x.Beneficiary!, x.Annex!, normalizedCode)).ToList();
    }

    public async Task<AnnexFoundationLineDto> CreateLineAsync(
        Guid declarationId,
        string annexCode,
        CreateAnnexFoundationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeAnnexCode(annexCode);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        ValidateRequest(request);

        var annex = await EnsureAnnexAsync(declarationId, normalizedCode, cancellationToken);
        var beneficiary = await EnsureBeneficiaryAsync(declarationId, request, cancellationToken);

        var line = new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            AnnexId = annex.Id,
            BeneficiaryId = beneficiary.Id,
            OperationType = request.OperationType.Trim(),
            GrossAmount = request.GrossAmount,
            TaxableAmount = request.GrossAmount,
            Rate = 0,
            WithheldAmount = request.WithholdingAmount,
            Notes = BuildNotes(request.OrderNumber, request.NetAmount, request.Notes),
            Status = DeclarationLineStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationLines.Add(line);
        AddAudit("ANNEX_FOUNDATION_LINE_CREATED", nameof(DeclarationLine), line.Id.ToString(), $"Creation ligne {normalizedCode} foundation.");
        AddEvent(declarationId, "ANNEX_FOUNDATION_LINE_CREATED", $"Creation ligne {normalizedCode} pour {beneficiary.FullNameOrCompanyName}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(line, beneficiary, annex, normalizedCode);
    }

    public async Task DeleteLineAsync(
        Guid declarationId,
        string annexCode,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeAnnexCode(annexCode);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var line = await QueryLines(declarationId, normalizedCode)
            .FirstOrDefaultAsync(x => x.Id == lineId, cancellationToken);

        if (line is null)
        {
            throw new ApplicationNotFoundException("Ligne annexe foundation introuvable.");
        }

        Db.DeclarationLines.Remove(line);
        AddAudit("ANNEX_FOUNDATION_LINE_DELETED", nameof(DeclarationLine), line.Id.ToString(), $"Suppression ligne {normalizedCode} foundation.");
        AddEvent(declarationId, "ANNEX_FOUNDATION_LINE_DELETED", $"Suppression ligne {normalizedCode}.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AnnexFoundationSummaryDto> GetSummaryAsync(
        Guid declarationId,
        string annexCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeAnnexCode(annexCode);
        await GetDeclarationAsync(declarationId, cancellationToken);

        var lines = await QueryLines(declarationId, normalizedCode)
            .ToListAsync(cancellationToken);

        return new AnnexFoundationSummaryDto
        {
            DeclarationId = declarationId,
            AnnexCode = normalizedCode,
            LinesCount = lines.Count,
            BeneficiariesCount = lines
                .Where(x => x.BeneficiaryId.HasValue)
                .Select(x => x.BeneficiaryId!.Value)
                .Distinct()
                .Count(),
            GrossAmountTotal = lines.Sum(x => x.GrossAmount),
            WithholdingAmountTotal = lines.Sum(x => x.WithheldAmount),
            NetAmountTotal = lines.Sum(x => x.GrossAmount - x.WithheldAmount),
            IsOfficialMappingConfirmed = false,
            MappingMessage = FiscalReferenceSeedService.OfficialMappingIncompleteMessage
        };
    }

    private IQueryable<DeclarationLine> QueryLines(Guid declarationId, string annexCode)
    {
        return Db.DeclarationLines
            .Include(x => x.Annex)
            .Include(x => x.Beneficiary)
            .Where(x => x.DeclarationId == declarationId
                && x.Annex != null
                && x.Annex.AnnexCode == annexCode)
            .OrderByDescending(x => x.CreatedAt);
    }

    private async Task<DeclarationAnnex> EnsureAnnexAsync(
        Guid declarationId,
        string annexCode,
        CancellationToken cancellationToken)
    {
        var annex = await Db.DeclarationAnnexes
            .FirstOrDefaultAsync(x => x.DeclarationId == declarationId && x.AnnexCode == annexCode, cancellationToken);

        if (annex is not null)
        {
            return annex;
        }

        annex = new DeclarationAnnex
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            AnnexCode = annexCode,
            Title = $"{annexCode} - Annexe foundation",
            Status = DeclarationAnnexStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationAnnexes.Add(annex);
        return annex;
    }

    private async Task<DeclarationBeneficiary> EnsureBeneficiaryAsync(
        Guid declarationId,
        CreateAnnexFoundationLineRequest request,
        CancellationToken cancellationToken)
    {
        var identifier = request.BeneficiaryIdentifier.Trim();

        var beneficiary = await Db.DeclarationBeneficiaries
            .FirstOrDefaultAsync(x => x.DeclarationId == declarationId && x.Identifier == identifier, cancellationToken);

        if (beneficiary is not null)
        {
            beneficiary.FullNameOrCompanyName = request.BeneficiaryName.Trim();
            beneficiary.UpdatedAt = DateTimeOffset.UtcNow;
            return beneficiary;
        }

        beneficiary = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            IdentifierType = BeneficiaryIdentifierType.Other,
            Identifier = identifier,
            FullNameOrCompanyName = request.BeneficiaryName.Trim(),
            IsResident = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationBeneficiaries.Add(beneficiary);
        return beneficiary;
    }

    private static string NormalizeAnnexCode(string annexCode)
    {
        var normalizedCode = annexCode.Trim().ToUpperInvariant();

        if (!AllowedAnnexCodes.Contains(normalizedCode))
        {
            throw new ApplicationConflictException("Annexe foundation invalide. Codes acceptes : A2, A3, A4, A5, A6, A7.");
        }

        return normalizedCode;
    }

    private static void ValidateRequest(CreateAnnexFoundationLineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BeneficiaryIdentifier))
        {
            throw new ApplicationConflictException("Identifiant beneficiaire obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.BeneficiaryName))
        {
            throw new ApplicationConflictException("Nom beneficiaire obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(request.OperationType))
        {
            throw new ApplicationConflictException("Type operation obligatoire.");
        }

        if (request.GrossAmount < 0 || request.WithholdingAmount < 0 || request.NetAmount < 0)
        {
            throw new ApplicationConflictException("Les montants doivent etre positifs ou nuls.");
        }
    }

    private static string? BuildNotes(int? orderNumber, decimal netAmount, string? notes)
    {
        var parts = new List<string>();

        if (orderNumber.HasValue)
        {
            parts.Add($"OrderNumber={orderNumber.Value}");
        }

        parts.Add($"NetAmount={netAmount:0.000}");

        if (!string.IsNullOrWhiteSpace(notes))
        {
            parts.Add(notes.Trim());
        }

        return string.Join(" | ", parts);
    }

    private static AnnexFoundationLineDto ToDto(
        DeclarationLine line,
        DeclarationBeneficiary beneficiary,
        DeclarationAnnex annex,
        string annexCode)
    {
        return new AnnexFoundationLineDto
        {
            Id = line.Id,
            DeclarationId = line.DeclarationId,
            AnnexId = annex.Id,
            BeneficiaryId = beneficiary.Id,
            AnnexCode = annexCode,
            BeneficiaryIdentifier = beneficiary.Identifier,
            BeneficiaryName = beneficiary.FullNameOrCompanyName,
            OperationType = line.OperationType,
            GrossAmount = line.GrossAmount,
            WithholdingAmount = line.WithheldAmount,
            NetAmount = line.GrossAmount - line.WithheldAmount,
            Notes = line.Notes,
            Status = line.Status.ToString(),
            IsOfficialMappingConfirmed = false
        };
    }
}

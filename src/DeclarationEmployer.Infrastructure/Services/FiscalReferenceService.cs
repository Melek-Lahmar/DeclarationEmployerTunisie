using DeclarationEmployer.Application.Fiscal;
using DeclarationEmployer.Contracts.Fiscal;
using DeclarationEmployer.Domain.Fiscal;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class FiscalReferenceService : IFiscalReferenceService
{
    private readonly ApplicationDbContext _db;

    public FiscalReferenceService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FiscalRuleSetDto>> GetRuleSetsAsync(
        int? year = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FiscalRuleSets
            .Include(x => x.Annexes)
            .AsNoTracking();

        if (year.HasValue)
        {
            query = query.Where(x => x.Year == year.Value);
        }

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        var ruleSets = await query
            .OrderByDescending(x => x.Year)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return ruleSets.Select(ToRuleSetDto).ToList();
    }

    public async Task<FiscalRuleSetDto?> GetRuleSetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ruleSet = await _db.FiscalRuleSets
            .Include(x => x.Annexes)
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return ruleSet is null ? null : ToRuleSetDto(ruleSet);
    }

    public async Task<IReadOnlyList<AnnexDefinitionDto>> GetAnnexesAsync(
        int? year = null,
        string? ruleSetCode = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AnnexDefinitions
            .Include(x => x.RuleSet)
            .Include(x => x.Fields)
            .AsNoTracking();

        if (year.HasValue)
        {
            query = query.Where(x => x.RuleSet != null && x.RuleSet.Year == year.Value);
        }

        if (!string.IsNullOrWhiteSpace(ruleSetCode))
        {
            var normalizedCode = ruleSetCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.RuleSet != null && x.RuleSet.Code == normalizedCode);
        }

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive && x.RuleSet != null && x.RuleSet.IsActive);
        }

        var annexes = await query
            .OrderBy(x => x.RuleSet!.Year)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return annexes.Select(ToAnnexDto).ToList();
    }

    public async Task<AnnexDefinitionDto?> GetAnnexAsync(
        int year,
        string annexCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeAnnexCode(annexCode);

        var annex = await _db.AnnexDefinitions
            .Include(x => x.RuleSet)
            .Include(x => x.Fields)
            .AsNoTracking()
            .Where(x => x.RuleSet != null && x.RuleSet.Year == year && x.Code == normalizedCode)
            .FirstOrDefaultAsync(cancellationToken);

        return annex is null ? null : ToAnnexDto(annex);
    }

    public async Task<IReadOnlyList<FiscalFieldDefinitionDto>> GetFieldsAsync(
        int year,
        string annexCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeAnnexCode(annexCode);

        var fields = await _db.FiscalFieldDefinitions
            .Include(x => x.AnnexDefinition)
            .ThenInclude(x => x!.RuleSet)
            .AsNoTracking()
            .Where(x => x.AnnexDefinition != null
                && x.AnnexDefinition.RuleSet != null
                && x.AnnexDefinition.RuleSet.Year == year
                && x.AnnexDefinition.Code == normalizedCode)
            .OrderBy(x => x.PositionStart ?? int.MaxValue)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return fields.Select(ToFieldDto).ToList();
    }

    public async Task<IReadOnlyList<FiscalRateDefinitionDto>> GetRatesAsync(
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FiscalRateDefinitions
            .Include(x => x.RuleSet)
            .AsNoTracking();

        if (year.HasValue)
        {
            query = query.Where(x => x.RuleSet != null && x.RuleSet.Year == year.Value);
        }

        var rates = await query
            .OrderBy(x => x.RuleSet!.Year)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return rates.Select(ToRateDto).ToList();
    }

    public async Task<FiscalReadinessDto> GetReadinessAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        var ruleSet = await _db.FiscalRuleSets
            .Include(x => x.Annexes)
            .ThenInclude(x => x.Fields)
            .AsNoTracking()
            .Where(x => x.Year == year && x.IsActive)
            .OrderBy(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);

        if (ruleSet is null)
        {
            return new FiscalReadinessDto
            {
                Year = year,
                HasActiveRuleSet = false,
                IsOfficialGenerationEnabled = false,
                Message = FiscalReferenceSeedService.OfficialMappingIncompleteMessage
            };
        }

        var fields = ruleSet.Annexes.SelectMany(x => x.Fields).ToList();
        var confirmedAnnexes = ruleSet.Annexes.Count(x => x.IsOfficialMappingConfirmed);
        var confirmedFields = fields.Count(x => x.IsConfirmed);
        var officialReady = ruleSet.Annexes.Count > 0
            && confirmedAnnexes == ruleSet.Annexes.Count
            && fields.Count > 0
            && confirmedFields == fields.Count;

        return new FiscalReadinessDto
        {
            Year = ruleSet.Year,
            RuleSetCode = ruleSet.Code,
            HasActiveRuleSet = true,
            IsOfficialGenerationEnabled = officialReady,
            Message = officialReady
                ? "Mapping fiscal officiel confirme."
                : FiscalReferenceSeedService.OfficialMappingIncompleteMessage,
            AnnexesCount = ruleSet.Annexes.Count,
            ConfirmedAnnexesCount = confirmedAnnexes,
            FieldsCount = fields.Count,
            ConfirmedFieldsCount = confirmedFields
        };
    }

    private static string NormalizeAnnexCode(string annexCode)
    {
        return annexCode.Trim().ToUpperInvariant();
    }

    private static FiscalRuleSetDto ToRuleSetDto(FiscalRuleSet ruleSet)
    {
        return new FiscalRuleSetDto
        {
            Id = ruleSet.Id,
            Year = ruleSet.Year,
            Code = ruleSet.Code,
            Name = ruleSet.Name,
            SourceName = ruleSet.SourceName,
            SourceReference = ruleSet.SourceReference,
            IsActive = ruleSet.IsActive,
            CreatedAt = ruleSet.CreatedAt,
            AnnexesCount = ruleSet.Annexes.Count,
            ConfirmedAnnexesCount = ruleSet.Annexes.Count(x => x.IsOfficialMappingConfirmed)
        };
    }

    private static AnnexDefinitionDto ToAnnexDto(AnnexDefinition annex)
    {
        return new AnnexDefinitionDto
        {
            Id = annex.Id,
            RuleSetId = annex.RuleSetId,
            Year = annex.RuleSet?.Year ?? 0,
            RuleSetCode = annex.RuleSet?.Code ?? string.Empty,
            Code = annex.Code,
            Name = annex.Name,
            Description = annex.Description,
            SortOrder = annex.SortOrder,
            IsActive = annex.IsActive,
            IsOfficialMappingConfirmed = annex.IsOfficialMappingConfirmed,
            Notes = annex.Notes,
            FieldsCount = annex.Fields.Count
        };
    }

    private static FiscalFieldDefinitionDto ToFieldDto(FiscalFieldDefinition field)
    {
        return new FiscalFieldDefinitionDto
        {
            Id = field.Id,
            AnnexDefinitionId = field.AnnexDefinitionId,
            AnnexCode = field.AnnexDefinition?.Code ?? string.Empty,
            Code = field.Code,
            Label = field.Label,
            DataType = field.DataType.ToString(),
            IsRequired = field.IsRequired,
            Length = field.Length,
            PositionStart = field.PositionStart,
            PositionEnd = field.PositionEnd,
            PaddingType = field.PaddingType.ToString(),
            DefaultValue = field.DefaultValue,
            SourceReference = field.SourceReference,
            IsConfirmed = field.IsConfirmed,
            Notes = field.Notes
        };
    }

    private static FiscalRateDefinitionDto ToRateDto(FiscalRateDefinition rate)
    {
        return new FiscalRateDefinitionDto
        {
            Id = rate.Id,
            RuleSetId = rate.RuleSetId,
            Year = rate.RuleSet?.Year ?? 0,
            RuleSetCode = rate.RuleSet?.Code ?? string.Empty,
            Code = rate.Code,
            Label = rate.Label,
            Rate = rate.Rate,
            EffectiveFrom = rate.EffectiveFrom,
            EffectiveTo = rate.EffectiveTo,
            SourceReference = rate.SourceReference,
            IsConfirmed = rate.IsConfirmed,
            Notes = rate.Notes
        };
    }
}

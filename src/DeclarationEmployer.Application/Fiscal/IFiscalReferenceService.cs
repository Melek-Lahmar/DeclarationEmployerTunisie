using DeclarationEmployer.Contracts.Fiscal;

namespace DeclarationEmployer.Application.Fiscal;

public interface IFiscalReferenceService
{
    Task<IReadOnlyList<FiscalRuleSetDto>> GetRuleSetsAsync(
        int? year = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<FiscalRuleSetDto?> GetRuleSetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnnexDefinitionDto>> GetAnnexesAsync(
        int? year = null,
        string? ruleSetCode = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<AnnexDefinitionDto?> GetAnnexAsync(
        int year,
        string annexCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FiscalFieldDefinitionDto>> GetFieldsAsync(
        int year,
        string annexCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FiscalRateDefinitionDto>> GetRatesAsync(
        int? year = null,
        CancellationToken cancellationToken = default);

    Task<FiscalReadinessDto> GetReadinessAsync(
        int year,
        CancellationToken cancellationToken = default);
}

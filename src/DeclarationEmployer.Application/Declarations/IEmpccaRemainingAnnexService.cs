using DeclarationEmployer.Contracts.Declarations.Empcca;

namespace DeclarationEmployer.Application.Declarations;

public interface IEmpccaRemainingAnnexService
{
    Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>>> GetA3LinesAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>> CreateA3LineAsync(Guid declarationId, CreateEmpccaAnnexA3LineRequest request, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>> UpdateA3LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA3LineRequest request, CancellationToken cancellationToken = default);
    Task DeleteA3LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexSummaryDto> GetA3SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexValidationDto> ValidateA3Async(Guid declarationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>>> GetA4LinesAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>> CreateA4LineAsync(Guid declarationId, CreateEmpccaAnnexA4LineRequest request, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>> UpdateA4LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA4LineRequest request, CancellationToken cancellationToken = default);
    Task DeleteA4LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexSummaryDto> GetA4SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexValidationDto> ValidateA4Async(Guid declarationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>>> GetA6LinesAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>> CreateA6LineAsync(Guid declarationId, CreateEmpccaAnnexA6LineRequest request, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>> UpdateA6LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA6LineRequest request, CancellationToken cancellationToken = default);
    Task DeleteA6LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexSummaryDto> GetA6SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexValidationDto> ValidateA6Async(Guid declarationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>>> GetA7LinesAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>> CreateA7LineAsync(Guid declarationId, CreateEmpccaAnnexA7LineRequest request, CancellationToken cancellationToken = default);
    Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>> UpdateA7LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA7LineRequest request, CancellationToken cancellationToken = default);
    Task DeleteA7LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexSummaryDto> GetA7SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexValidationDto> ValidateA7Async(Guid declarationId, CancellationToken cancellationToken = default);
}

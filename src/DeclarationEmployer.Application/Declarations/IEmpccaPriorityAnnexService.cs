using DeclarationEmployer.Contracts.Declarations.Empcca;

namespace DeclarationEmployer.Application.Declarations;

public interface IEmpccaPriorityAnnexService
{
    Task<IReadOnlyList<EmpccaAnnexA1LineDto>> GetA1LinesAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexA1LineDto> CreateA1LineAsync(Guid declarationId, CreateEmpccaAnnexA1LineRequest request, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexA1SummaryDto> GetA1SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexValidationDto> ValidateA1Async(Guid declarationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmpccaAnnexA2LineDto>> GetA2LinesAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexA2LineDto> CreateA2LineAsync(Guid declarationId, CreateEmpccaAnnexA2LineRequest request, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexA2SummaryDto> GetA2SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexValidationDto> ValidateA2Async(Guid declarationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmpccaAnnexA5LineDto>> GetA5LinesAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexA5LineDto> CreateA5LineAsync(Guid declarationId, CreateEmpccaAnnexA5LineRequest request, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexA5SummaryDto> GetA5SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default);
    Task<EmpccaAnnexValidationDto> ValidateA5Async(Guid declarationId, CancellationToken cancellationToken = default);
}

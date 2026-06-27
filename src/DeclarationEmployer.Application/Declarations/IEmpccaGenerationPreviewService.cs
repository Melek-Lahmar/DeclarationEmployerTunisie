using DeclarationEmployer.Contracts.Generation;

namespace DeclarationEmployer.Application.Declarations;

public interface IEmpccaGenerationPreviewService
{
    Task<EmpccaGenerationPreviewDto> PreviewAsync(Guid declarationId, CancellationToken cancellationToken = default);
}

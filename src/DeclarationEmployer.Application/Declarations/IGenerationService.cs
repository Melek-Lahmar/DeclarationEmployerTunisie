using DeclarationEmployer.Contracts.Generation;

namespace DeclarationEmployer.Application.Declarations;

public interface IGenerationService
{
    Task<GenerationResultDto> GenerateAsync(
        Guid declarationId,
        GenerateDeclarationFilesRequest request,
        CancellationToken cancellationToken = default);
}

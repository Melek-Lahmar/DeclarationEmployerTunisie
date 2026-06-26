using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationBeneficiariesService
{
    Task<IReadOnlyList<DeclarationBeneficiaryDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default);

    Task<DeclarationBeneficiaryDto> CreateAsync(Guid declarationId, CreateDeclarationBeneficiaryRequest request, CancellationToken cancellationToken = default);

    Task<DeclarationBeneficiaryDto> UpdateAsync(Guid declarationId, Guid beneficiaryId, UpdateDeclarationBeneficiaryRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid declarationId, Guid beneficiaryId, CancellationToken cancellationToken = default);
}

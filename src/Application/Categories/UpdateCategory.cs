using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;

using Domain.Users;
using ShareKernal;
using static Application.Categories.CategoryErrors;
using static ShareKernal.Result;

namespace Application.Categories;

public record UpdateCategoryCommandRequest(CategoryId CategoryId, UserId UserId, string Name)
    : IRequest<Result<UpdateCategoryCommandResponse>>;

public record UpdateCategoryCommandResponse(Guid CategoryId, string Name);

public class UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommandRequest, Result<UpdateCategoryCommandResponse>>
{
    public async Task<Result<UpdateCategoryCommandResponse>> Handle(UpdateCategoryCommandRequest command, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAndUserIdAsync(command.CategoryId, command.UserId, cancellationToken);

        if (category is null)
            return NotFound;

        var existing = await categoryRepository.GetAllByUserIdAsync(command.UserId, cancellationToken);
        if (existing.Any(c => c.Id != command.CategoryId.Value && string.Equals(c.Name, command.Name, StringComparison.OrdinalIgnoreCase)))
            return DuplicateName;

        var renameResult = category.Rename(command.Name);
        if (!renameResult.IsSuccess) return renameResult.Error;

        await categoryRepository.Update(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new UpdateCategoryCommandResponse(category.Id.Value, category.Name));
    }
}

using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

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
            return CategoryErrors.NotFound;

        var existing = await categoryRepository.GetAllByUserIdAsync(command.UserId, cancellationToken);
        if (existing.Any(c => c.Id != command.CategoryId.Value && string.Equals(c.Name, command.Name, StringComparison.OrdinalIgnoreCase)))
            return CategoryErrors.DuplicateName;

        category.Rename(command.Name);
        await categoryRepository.UpdateAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateCategoryCommandResponse(category.Id.Value, category.Name));
    }
}

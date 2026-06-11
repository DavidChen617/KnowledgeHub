using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;

using Domain.Users;
using ShareKernal;
using static Application.Categories.CategoryErrors;
using static ShareKernal.Result;

namespace Application.Categories;

public record UpdateCategoryCommand(CategoryId CategoryId, UserId UserId, string Name)
    : IRequest<Result<UpdateCategoryDto>>;

public record UpdateCategoryDto(Guid CategoryId, string Name);

public class UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryDto>>
{
    public async Task<Result<UpdateCategoryDto>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
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

        return Success(new UpdateCategoryDto(category.Id.Value, category.Name));
    }
}

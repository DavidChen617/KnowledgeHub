using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Exceptions;
using Domain.Shared;
using Domain.Users;

namespace Application.Categories;

public record UpdateCategoryCommandRequest(CategoryId CategoryId, UserId UserId, string Name)
    : IRequest<UpdateCategoryCommandResponse>;

public record UpdateCategoryCommandResponse(Guid CategoryId, string Name);

public class UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommandRequest, UpdateCategoryCommandResponse?>
{
    public async Task<UpdateCategoryCommandResponse?> Handle(UpdateCategoryCommandRequest command, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAndUserIdAsync(command.CategoryId, command.UserId, cancellationToken);

        if (category is null)
            return null;

        var existing = await categoryRepository.GetAllByUserIdAsync(command.UserId, cancellationToken);
        if (existing.Any(c => c.Id != command.CategoryId.Value && string.Equals(c.Name, command.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DuplicateCategoryNameException(command.Name);

        category.Rename(command.Name);
        await categoryRepository.UpdateAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryCommandResponse(category.Id.Value, category.Name);
    }
}

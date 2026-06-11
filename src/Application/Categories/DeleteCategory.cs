using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;

using Domain.Users;
using ShareKernal;
using static Application.Categories.CategoryErrors;
using static ShareKernal.Result;

namespace Application.Categories;

public record DeleteCategoryCommand(CategoryId CategoryId, UserId UserId)
    : IRequest<Result>;

public class DeleteCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAndUserIdAsync(command.CategoryId, command.UserId, cancellationToken);

        if (category is null)
            return NotFound;

        if (await categoryRepository.IsInUseAsync(command.CategoryId, cancellationToken))
            return InUse;

        category.Delete();
        await categoryRepository.DeleteAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}

using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Exceptions;
using Domain.Shared;
using Domain.Users;

namespace Application.Categories;

public record DeleteCategoryCommandRequest(CategoryId CategoryId, UserId UserId)
    : IRequest<DeleteCategoryCommandResponse>;

public record DeleteCategoryCommandResponse;

public class DeleteCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommandRequest, DeleteCategoryCommandResponse?>
{
    public async Task<DeleteCategoryCommandResponse?> Handle(DeleteCategoryCommandRequest command, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAndUserIdAsync(command.CategoryId, command.UserId, cancellationToken);

        if (category is null)
            return null;

        if (await categoryRepository.IsInUseAsync(command.CategoryId, cancellationToken))
            throw new CategoryInUseException();

        await categoryRepository.DeleteAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryCommandResponse();
    }
}

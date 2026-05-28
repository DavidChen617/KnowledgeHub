using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Categories;

public record AddCategoryCommandRequest(UserId UserId, string Name)
    : IRequest<Result<AddCategoryCommandResponse>>;

public record AddCategoryCommandResponse(Guid CategoryId, string Name);

public class AddCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<AddCategoryCommandRequest, Result<AddCategoryCommandResponse>>
{
    public async Task<Result<AddCategoryCommandResponse>> Handle(AddCategoryCommandRequest command, CancellationToken cancellationToken = default)
    {
        var existing = await categoryRepository.GetAllByUserIdAsync(command.UserId, cancellationToken);

        if (existing.Any(c => string.Equals(c.Name, command.Name, StringComparison.OrdinalIgnoreCase)))
            return CategoryErrors.DuplicateName;

        var category = Category.Create(command.UserId, command.Name);

        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AddCategoryCommandResponse(category.Id.Value, category.Name));
    }
}

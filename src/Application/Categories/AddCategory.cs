using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Shared;
using Domain.Users;
using ShareKernal;
using static Application.Categories.CategoryErrors;
using static ShareKernal.Result;

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
            return DuplicateName;

        var categoryResult = Category.Create(command.UserId, command.Name);
        if (!categoryResult.IsSuccess) return categoryResult.Error;

        var category = categoryResult.Value;

        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new AddCategoryCommandResponse(category.Id.Value, category.Name));
    }
}

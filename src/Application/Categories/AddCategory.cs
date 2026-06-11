using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;

using Domain.Users;
using ShareKernal;
using static Application.Categories.CategoryErrors;
using static ShareKernal.Result;

namespace Application.Categories;

public record AddCategoryCommand(UserId UserId, string Name)
    : IRequest<Result<AddCategoryDto>>;

public record AddCategoryDto(Guid Id, string Name);

public class AddCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<AddCategoryCommand, Result<AddCategoryDto>>
{
    public async Task<Result<AddCategoryDto>> Handle(AddCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await categoryRepository.GetAllByUserIdAsync(command.UserId, cancellationToken);

        if (existing.Any(c => string.Equals(c.Name, command.Name, StringComparison.OrdinalIgnoreCase)))
            return DuplicateName;

        var categoryResult = Category.Create(command.UserId, command.Name);
        if (!categoryResult.IsSuccess) return categoryResult.Error;

        var category = categoryResult.Value;

        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new AddCategoryDto(category.Id.Value, category.Name));
    }
}

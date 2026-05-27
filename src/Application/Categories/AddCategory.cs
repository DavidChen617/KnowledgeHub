using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Exceptions;
using Domain.Users;

namespace Application.Categories;

public record AddCategoryCommandRequest(UserId UserId, string Name)
    : IRequest<AddCategoryCommandResponse>;

public record AddCategoryCommandResponse(CategoryId CategoryId, string Name);

public class AddCategoryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<AddCategoryCommandRequest, AddCategoryCommandResponse>
{
    public async Task<AddCategoryCommandResponse> Handle(AddCategoryCommandRequest command, CancellationToken cancellationToken = default)
    {
        var existing = await categoryRepository.GetAllByUserIdAsync(command.UserId, cancellationToken);

        if (existing.Any(c => string.Equals(c.Name, command.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DuplicateCategoryNameException(command.Name);

        var category = Category.Create(command.UserId, command.Name);

        await categoryRepository.AddAsync(category, cancellationToken);

        return new AddCategoryCommandResponse(category.Id, category.Name);
    }
}

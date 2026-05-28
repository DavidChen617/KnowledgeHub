using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Users;
using ShareKernal;

namespace Application.Categories;

public record ListCategoriesQueryRequest(UserId UserId)
    : IRequest<Result<ListCategoriesQueryResponse>>;

public record ListCategoriesQueryResponse(IReadOnlyList<CategorySummary> Categories);

public class ListCategoriesHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<ListCategoriesQueryRequest, Result<ListCategoriesQueryResponse>>
{
    public async Task<Result<ListCategoriesQueryResponse>> Handle(ListCategoriesQueryRequest query, CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);

        return Result.Success(new ListCategoriesQueryResponse(categories));
    }
}

using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Users;

namespace Application.Categories;

public record ListCategoriesQueryRequest(UserId UserId)
    : IRequest<ListCategoriesQueryResponse>;

public record ListCategoriesQueryResponse(IReadOnlyList<CategorySummary> Categories);

public class ListCategoriesHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<ListCategoriesQueryRequest, ListCategoriesQueryResponse>
{
    public async Task<ListCategoriesQueryResponse> Handle(ListCategoriesQueryRequest query, CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);

        return new ListCategoriesQueryResponse(categories);
    }
}

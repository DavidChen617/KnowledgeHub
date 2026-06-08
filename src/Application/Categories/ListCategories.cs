using Application.Interfaces;
using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Users;
using ShareKernal;
using static ShareKernal.Result;

namespace Application.Categories;

public record ListCategoriesQueryRequest(UserId UserId)
    : IRequest<Result<ListCategoriesQueryResponse>>;

public record ListCategoriesQueryResponse(IReadOnlyList<CategorySummary> Categories);

public class ListCategoriesHandler(ICategoryRepository categoryRepository, ICacher cacher)
    : IRequestHandler<ListCategoriesQueryRequest, Result<ListCategoriesQueryResponse>>
{
    public async Task<Result<ListCategoriesQueryResponse>> Handle(ListCategoriesQueryRequest query, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Categories(query.UserId.Value);

        var cached = await cacher.GetAsync<ListCategoriesQueryResponse>(key, cancellationToken);
        if (cached is not null) return Success(cached);

        var categories = await categoryRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);
        var response = new ListCategoriesQueryResponse(categories);

        await cacher.SetAsync(key, response, TimeSpan.FromMinutes(10), cancellationToken);

        return Success(response);
    }
}

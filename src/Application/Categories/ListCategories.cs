using Application.Interfaces;
using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Users;
using ShareKernal;
using static ShareKernal.Result;

namespace Application.Categories;

public record ListCategoriesQuery(UserId UserId)
    : IRequest<Result<ListCategoriesDto>>;

public record ListCategoriesDto(IReadOnlyList<CategorySummary> Categories);

public class ListCategoriesHandler(ICategoryRepository categoryRepository, ICacher cacher)
    : IRequestHandler<ListCategoriesQuery, Result<ListCategoriesDto>>
{
    public async Task<Result<ListCategoriesDto>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Categories(query.UserId.Value);

        var cached = await cacher.GetAsync<ListCategoriesDto>(key, cancellationToken);
        if (cached is not null) return Success(cached);

        var categories = await categoryRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);
        var response = new ListCategoriesDto(categories);

        await cacher.SetAsync(key, response, TimeSpan.FromMinutes(10), cancellationToken);

        return Success(response);
    }
}

using Domain.Categories;
using Domain.Shared;

namespace Domain.Exceptions;

public class CategoryNotFoundException(CategoryId categoryId)
    : DomainException($"找不到分類（ID：{categoryId.Value}）");

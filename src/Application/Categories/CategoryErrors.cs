using ShareKernal;

namespace Application.Categories;

public static class CategoryErrors
{
    public static readonly Error NotFound      = new("Category.NotFound",      "Category not found",        ErrorType.NotFound);
    public static readonly Error DuplicateName = new("Category.DuplicateName", "Category name already exists", ErrorType.Conflict);
    public static readonly Error InUse         = new("Category.InUse",         "Category is still in use",  ErrorType.Conflict);
}

using Domain.Shared;

namespace Domain.Exceptions;

public class CategoryInUseException()
    : DomainException("此分類仍有筆記引用，無法刪除");

public class DuplicateCategoryNameException(string name)
    : DomainException($"分類名稱「{name}」已存在");

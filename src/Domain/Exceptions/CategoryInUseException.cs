namespace Domain.Exceptions;

public class CategoryInUseException()
    : Exception("此分類仍有筆記引用，無法刪除");

public class DuplicateCategoryNameException(string name)
    : Exception($"分類名稱「{name}」已存在");

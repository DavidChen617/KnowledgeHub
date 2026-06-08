using Domain.Categories;
using Domain.Categories.Events;
using Domain.Users;

namespace UnitTests.Domain;

public class CategoryTests
{
    [Fact]
    public void Given_EmptyName_When_CreateCategory_Then_ReturnsError()
    {
        var result = Category.Create(UserId.New(), "   ");

        Assert.False(result.IsSuccess);
        Assert.Equal(Category.Errors.EmptyName.Code, result.Error.Code);
    }

    [Fact]
    public void Given_ValidName_When_CreateCategory_Then_RaisesCategoryCreatedEvent()
    {
        var result = Category.Create(UserId.New(), "筆記分類");

        Assert.True(result.IsSuccess);
        Assert.Equal("筆記分類", result.Value.Name);
        Assert.Single(result.Value.DomainEvents.OfType<CategoryCreatedEvent>());
    }

    [Fact]
    public void Given_EmptyName_When_Rename_Then_ReturnsError()
    {
        var category = Category.Create(UserId.New(), "原始名稱").Value;

        var result = category.Rename("");

        Assert.False(result.IsSuccess);
        Assert.Equal("原始名稱", category.Name);
    }

    [Fact]
    public void Given_ValidName_When_Rename_Then_UpdatesNameAndRaisesEvent()
    {
        var category = Category.Create(UserId.New(), "原始名稱").Value;
        category.ClearDomainEvents();

        var result = category.Rename("新名稱");

        Assert.True(result.IsSuccess);
        Assert.Equal("新名稱", category.Name);
        Assert.Single(category.DomainEvents.OfType<CategoryUpdatedEvent>());
    }
}

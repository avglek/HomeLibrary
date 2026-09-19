using HomeLibrary.Models;

namespace HomeLibrary.Tests.Fixtures;

/// <summary>
/// Фабрика тестовых данных.
/// </summary>
public static class TestData
{
    public static Book SampleBook(int id = 1, string title = "Война и мир") => new()
    {
        Id = id,
        Title = title,
        Author = "Лев Толстой",
        PublishYear = 1869,
        TocContent = "<toc><h1>Том первый</h1></toc>",
        CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
    };

    public static BookCreateDto ValidCreateDto() => new()
    {
        Title = "Преступление и наказание",
        Author = "Фёдор Достоевский",
        PublishYear = 1866,
        TocContent = "<toc><h1>Часть первая</h1></toc>"
    };

    public static BookCreateDto EmptyTocDto() => new()
    {
        Title = "Книга без оглавления",
        Author = "Неизвестный автор",
        PublishYear = 2020,
        TocContent = null
    };
}
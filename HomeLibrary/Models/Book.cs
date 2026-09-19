namespace HomeLibrary.Models;

/// <summary>
/// Книга домашней библиотеки.
/// </summary>
public class Book
{
    /// <summary>Уникальный идентификатор.</summary>
    public int Id { get; set; }

    /// <summary>Название книги.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Автор.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Год издания (может быть null, если неизвестен).</summary>
    public int? PublishYear { get; set; }

    /// <summary>
    /// Оглавление в виде XML-документа.
    /// В БД хранится как тип xml, но Dapper маппит его в string.
    /// </summary>
    public string TocContent { get; set; } = string.Empty;

    /// <summary>Дата добавления в библиотеку.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Дата последнего изменения.</summary>
    public DateTime UpdatedAt { get; set; }
}
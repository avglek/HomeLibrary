using HomeLibrary.Models;

namespace HomeLibrary.Services;

/// <summary>
/// Валидатор данных книги. Возвращает список ошибок.
/// </summary>
public interface IBookValidator
{
    IReadOnlyList<string> Validate(BookCreateDto dto);
}
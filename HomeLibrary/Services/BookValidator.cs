using HomeLibrary.Models;

namespace HomeLibrary.Services;

/// <summary>
/// Реализация валидатора: правила бизнес-уровня.
/// </summary>
public sealed class BookValidator : IBookValidator
{
    private const int MaxTitleLength = 255;
    private const int MaxAuthorLength = 255;
    private const int MinYear = 0;
    private const int MaxYear = 2100;

    public IReadOnlyList<string> Validate(BookCreateDto dto)
    {
        var errors = new List<string>();

        if (dto is null)
        {
            errors.Add("Тело запроса пустое");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
            errors.Add("Название обязательно");
        else if (dto.Title.Length > MaxTitleLength)
            errors.Add($"Название не должно превышать {MaxTitleLength} символов");

        if (string.IsNullOrWhiteSpace(dto.Author))
            errors.Add("Автор обязателен");
        else if (dto.Author.Length > MaxAuthorLength)
            errors.Add($"Имя автора не должно превышать {MaxAuthorLength} символов");

        if (dto.PublishYear is < MinYear or > MaxYear)
            errors.Add($"Год издания должен быть в диапазоне {MinYear}–{MaxYear}");

        return errors;
    }
}
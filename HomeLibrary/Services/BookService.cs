using HomeLibrary.Models;
using HomeLibrary.Repositories;
using Microsoft.Extensions.Logging;

namespace HomeLibrary.Services;

/// <summary>
/// Бизнес-логика работы с книгами.
/// Отвечает за валидацию, нормализацию и координацию репозитория.
/// </summary>
public sealed class BookService : IBookService
{
    private readonly IBookRepository _repository;
    private readonly IBookValidator _validator;
    private readonly ILogger<BookService> _logger;

    public BookService(
        IBookRepository repository,
        IBookValidator validator,
        ILogger<BookService> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);

    public Task<IEnumerable<Book>> SearchAsync(string term, CancellationToken ct = default)
        => _repository.SearchAsync(term?.Trim() ?? string.Empty, ct);

    public Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public Task<int> CreateAsync(BookCreateDto dto, CancellationToken ct = default)
    {
        EnsureValid(dto);
        Normalize(dto);
        _logger.LogInformation("Создание книги «{Title}»", dto.Title);
        return _repository.CreateAsync(dto, ct);
    }

    public Task<bool> UpdateAsync(int id, BookCreateDto dto, CancellationToken ct = default)
    {
        EnsureValid(dto);
        Normalize(dto);
        _logger.LogInformation("Обновление книги id={Id}", id);
        return _repository.UpdateAsync(id, dto, ct);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);

    // ─────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────

    private void EnsureValid(BookCreateDto dto)
    {
        var errors = _validator.Validate(dto);
        if (errors.Count > 0)
        {
            _logger.LogWarning("Валидация не пройдена: {Errors}", string.Join("; ", errors));
            throw new ValidationException(errors);
        }
    }

    private static void Normalize(BookCreateDto dto)
    {
        dto.Title = dto.Title.Trim();
        dto.Author = dto.Author.Trim();

        if (string.IsNullOrWhiteSpace(dto.TocContent))
            dto.TocContent = "<toc/>";
    }
}

/// <summary>
/// Исключение бизнес-валидации. Middleware превращает его в 400 Bad Request.
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IReadOnlyList<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors;
    }
}
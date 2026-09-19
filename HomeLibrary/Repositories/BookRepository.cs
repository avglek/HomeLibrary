using Dapper;
using HomeLibrary.Data;
using HomeLibrary.Models;
using Microsoft.Extensions.Logging;

namespace HomeLibrary.Repositories;

/// <summary>
/// Реализация репозитория книг через Dapper + PostgreSQL.
/// </summary>
public sealed class BookRepository : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<BookRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Получение всех книг");
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT * FROM get_all_books()",
            cancellationToken: ct);
        return await connection.QueryAsync<Book>(command);
    }

    public async Task<IEnumerable<Book>> SearchAsync(string term, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync(ct);

        _logger.LogDebug("Поиск книг по запросу: {Term}", term);
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT * FROM search_books(@SearchTerm)",
            new { SearchTerm = term },
            cancellationToken: ct);
        return await connection.QueryAsync<Book>(command);
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Получение книги id={Id}", id);
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT * FROM get_book_by_id(@Id)",
            new { Id = id },
            cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<Book>(command);
    }

    public async Task<int> CreateAsync(BookCreateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Создание книги «{Title}»", dto.Title);
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT insert_book(@Title, @Author, @PublishYear, @TocContent)",
            new { dto.Title, dto.Author, dto.PublishYear, dto.TocContent },
            cancellationToken: ct);
        return await connection.QuerySingleAsync<int>(command);
    }

    public async Task<bool> UpdateAsync(int id, BookCreateDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Обновление книги id={Id}", id);
        using var connection = _connectionFactory.CreateConnection();

        var existsCommand = new CommandDefinition(
            "SELECT COUNT(1) FROM books WHERE id = @Id",
            new { Id = id },
            cancellationToken: ct);

        var exists = await connection.ExecuteScalarAsync<int>(existsCommand);
        if (exists == 0)
        {
            _logger.LogWarning("Книга id={Id} не найдена при обновлении", id);
            return false;
        }

        var updateCommand = new CommandDefinition(
            "SELECT update_book(@Id, @Title, @Author, @PublishYear, @TocContent)",
            new { Id = id, dto.Title, dto.Author, dto.PublishYear, dto.TocContent },
            cancellationToken: ct);

        await connection.ExecuteAsync(updateCommand);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Удаление книги id={Id}", id);
        using var connection = _connectionFactory.CreateConnection();

        var existsCommand = new CommandDefinition(
            "SELECT COUNT(1) FROM books WHERE id = @Id",
            new { Id = id },
            cancellationToken: ct);

        var exists = await connection.ExecuteScalarAsync<int>(existsCommand);
        if (exists == 0)
        {
            _logger.LogWarning("Книга id={Id} не найдена при удалении", id);
            return false;
        }

        var deleteCommand = new CommandDefinition(
            "SELECT delete_book(@Id)",
            new { Id = id },
            cancellationToken: ct);

        await connection.ExecuteAsync(deleteCommand);
        return true;
    }
}
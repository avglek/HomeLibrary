using HomeLibrary.Models;

namespace HomeLibrary.Repositories;

/// <summary>
/// Репозиторий книг. Все операции — через хранимые процедуры PostgreSQL.
/// </summary>
public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Book>> SearchAsync(string term, CancellationToken ct = default);
    Task<Book?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(BookCreateDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, BookCreateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
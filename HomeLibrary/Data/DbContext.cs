using System.Data;
using Npgsql;

namespace HomeLibrary.Data;

/// <summary>
/// Фабрика подключений к PostgreSQL.
/// Регистрируется в DI как Scoped — создаётся один раз на HTTP-запрос.
/// </summary>
public class DbContext
{
    private readonly string _connectionString;

    public DbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Строка подключения 'DefaultConnection' не найдена в конфигурации.");
    }

    /// <summary>
    /// Создаёт новое подключение к БД. Вызывающий код обязан освободить его
    /// (using var connection = db.CreateConnection();).
    /// </summary>
    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
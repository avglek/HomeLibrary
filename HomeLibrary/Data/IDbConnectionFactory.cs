using System.Data;

namespace HomeLibrary.Data;

/// <summary>
/// Фабрика подключений к БД. Реализации могут быть разными
/// (Npgsql, InMemory, тестовые).
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>Создаёт новое открытое подключение. Вызывающий обязан освободить его.</summary>
    IDbConnection CreateConnection();
}
using System.Data;
using HomeLibrary.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HomeLibrary.Data;

/// <summary>
/// Реализация фабрики подключений на базе Npgsql.
/// </summary>
public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.DefaultConnection
            ?? throw new InvalidOperationException(
                "DatabaseOptions.DefaultConnection не задан.");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
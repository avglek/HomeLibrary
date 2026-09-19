using HomeLibrary.Configuration;
using HomeLibrary.Data;
using HomeLibrary.Repositories;
using Microsoft.Extensions.Options;

namespace HomeLibrary.Configuration;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Регистрирует инфраструктурные сервисы: фабрику подключений и репозитории.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options с валидацией на старте
        services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        // Фабрика подключений — stateless, поэтому Singleton
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // Репозитории — Scoped, потому что на каждый запрос создаётся подключение
        services.AddScoped<IBookRepository, BookRepository>();

        return services;
    }
}
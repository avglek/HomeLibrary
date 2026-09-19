using HomeLibrary.Services;

namespace HomeLibrary.Configuration;

public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Регистрирует сервисы бизнес-логики.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookValidator, BookValidator>();
        services.AddScoped<IBookService, BookService>();
        return services;
    }
}
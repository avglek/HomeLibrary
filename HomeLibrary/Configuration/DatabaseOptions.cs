using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Configuration;

/// <summary>
/// Настройки подключения к БД. Привязываются к секции "ConnectionStrings".
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required(ErrorMessage = "Строка подключения 'DefaultConnection' обязательна")]
    public string DefaultConnection { get; set; } = string.Empty;
}
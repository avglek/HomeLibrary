using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Models;

/// <summary>
/// DTO для создания/обновления книги.
/// </summary>
public class BookCreateDto
{
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(255, ErrorMessage = "Название не должно превышать 255 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Автор обязателен")]
    [StringLength(255, ErrorMessage = "Имя автора не должно превышать 255 символов")]
    public string Author { get; set; } = string.Empty;

    [Range(0, 2100, ErrorMessage = "Год издания должен быть в диапазоне 0–2100")]
    public int? PublishYear { get; set; }

    /// <summary>
    /// XML-строка с оглавлением. Может быть пустой — тогда сохраняется как &lt;toc/&gt;.
    /// </summary>
    public string? TocContent { get; set; }
}
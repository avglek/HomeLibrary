using FluentAssertions;
using HomeLibrary.Models;
using HomeLibrary.Repositories;
using HomeLibrary.Services;
using HomeLibrary.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HomeLibrary.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _repoMock = new(MockBehavior.Strict);
    private readonly IBookValidator _validator = new BookValidator();
    private readonly BookService _service;

    public BookServiceTests()
    {
        _service = new BookService(
            _repoMock.Object,
            _validator,
            NullLogger<BookService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var books = new List<Book> { TestData.SampleBook() };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(books);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task SearchAsync_TrimsTerm()
    {
        _repoMock.Setup(r => r.SearchAsync("Толстой", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Array.Empty<Book>());

        await _service.SearchAsync("   Толстой   ");

        _repoMock.Verify(r => r.SearchAsync("Толстой", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyToc_BecomesDefault()
    {
        var dto = new BookCreateDto
        {
            Title = "Книга",
            Author = "Автор",
            TocContent = null
        };

        BookCreateDto? captured = null;
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<BookCreateDto>(), It.IsAny<CancellationToken>()))
                 .Callback<BookCreateDto, CancellationToken>((d, _) => captured = d)
                 .ReturnsAsync(1);

        await _service.CreateAsync(dto);

        captured!.TocContent.Should().Be("<toc/>");
    }

    [Fact]
    public async Task CreateAsync_InvalidDto_ThrowsValidationException()
    {
        var dto = new BookCreateDto { Title = "", Author = "" };

        var act = () => _service.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
                 .Where(e => e.Errors.Count >= 2);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
    {
        _repoMock.Setup(r => r.UpdateAsync(99, It.IsAny<BookCreateDto>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        var ok = await _service.UpdateAsync(99, TestData.ValidCreateDto());

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        _repoMock.Setup(r => r.DeleteAsync(7, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var ok = await _service.DeleteAsync(7);

        ok.Should().BeTrue();
    }
}
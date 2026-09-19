using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HomeLibrary.Models;
using HomeLibrary.Repositories;
using HomeLibrary.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace HomeLibrary.Tests.Api;

public class BooksApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly Mock<IBookRepository> _repoMock;

    // Конструктор принимает фикстуру — xUnit сам её создаст
    public BooksApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _repoMock = factory.RepositoryMock; // мок лежит внутри фикстуры
    }

    [Fact]
    public async Task GetBooks_ReturnsOk_WithListOfBooks()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Book>
             {
             TestData.SampleBook(1, "Война и мир"),
             TestData.SampleBook(2, "Анна Каренина")
             });

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/books");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

    }

    [Fact]
    public async Task GetBooks_WithSearch_InvokesSearchOnRepository()
    {
        // Arrange
        const string searchTerm = "Толстой";

        _repoMock.Setup(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Book> { TestData.SampleBook() });

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/books?search={Uri.EscapeDataString(searchTerm)}");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        _repoMock.Verify(
            r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/books/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookById_ReturnsOk_WhenExists()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(TestData.SampleBook(1));

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/books/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var book = await response.Content.ReadFromJsonAsync<Book>();
        book!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetBookById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Book?)null);

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/books/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────
    // POST /api/books
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBook_ReturnsCreated_WithNewId()
    {
        // Arrange
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<BookCreateDto>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(42);

        var client = _factory.CreateClient();
        var dto = TestData.ValidCreateDto();

        // Act
        var response = await client.PostAsJsonAsync("/api/books", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should().EndWith("/api/books/42");
    }

    [Fact]
    public async Task CreateBook_ReturnsBadRequest_WhenTitleMissing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var dto = new BookCreateDto
        {
            Title = "",       // пусто
            Author = "Автор"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/books", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────
    // PUT /api/books/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateBook_ReturnsNoContent_WhenUpdated()
    {
        // Arrange
        _repoMock.Setup(r => r.UpdateAsync(1, It.IsAny<BookCreateDto>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var client = _factory.CreateClient();
        var dto = TestData.ValidCreateDto();

        // Act
        var response = await client.PutAsJsonAsync("/api/books/1", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _repoMock.Setup(r => r.UpdateAsync(999, It.IsAny<BookCreateDto>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/books/999", TestData.ValidCreateDto());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────
    // DELETE /api/books/{id}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/books/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/books/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
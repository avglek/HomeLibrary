using HomeLibrary.Models;
using HomeLibrary.Services;
using Npgsql;

namespace HomeLibrary.Endpoints;

/// <summary>
/// Эндпоинты API для книг.
/// </summary>
public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/books")
                     .WithTags("Books");

        // GET /api/books  или  /api/books?search=...
        api.MapGet("/", async (
            IBookService service,
            CancellationToken ct,
            string? search = null) =>
        {
            var books = string.IsNullOrWhiteSpace(search)
                ? await service.GetAllAsync(ct)
                : await service.SearchAsync(search, ct);

            return Results.Ok(books);
        })
        .WithName("GetBooks")
        .WithSummary("Список книг или поиск по названию/автору/оглавлению")
        .Produces<IEnumerable<Book>>(StatusCodes.Status200OK);

        // GET /api/books/{id}
        api.MapGet("/{id:int}", async (
            int id,
            IBookService service,
            CancellationToken ct) =>
        {
            var book = await service.GetByIdAsync(id, ct);
            return book is not null
                ? Results.Ok(book)
                : Results.NotFound(new { message = $"Книга с id={id} не найдена" });
        })
        .WithName("GetBookById")
        .Produces<Book>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/books
        api.MapPost("/", async (
            BookCreateDto dto,
            IBookService service,
            CancellationToken ct) =>
        {
            try
            {
                var newId = await service.CreateAsync(dto, ct);
                return Results.Created($"/api/books/{newId}", new { id = newId });
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "2200N")
            {
                return Results.BadRequest(new
                {
                    message = "Оглавление должно быть корректным XML",
                    detail = pgEx.MessageText
                });
            }
        })
        .WithName("CreateBook")
        .Accepts<BookCreateDto>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // PUT /api/books/{id}
        api.MapPut("/{id:int}", async (
            int id,
            BookCreateDto dto,
            IBookService service,
            CancellationToken ct) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, dto, ct);
                return updated
                    ? Results.NoContent()
                    : Results.NotFound(new { message = $"Книга с id={id} не найдена" });
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "2200N")
            {
                return Results.BadRequest(new
                {
                    message = "Оглавление должно быть корректным XML",
                    detail = pgEx.MessageText
                });
            }
        })
        .WithName("UpdateBook")
        .Accepts<BookCreateDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        // DELETE /api/books/{id}
        api.MapDelete("/{id:int}", async (
            int id,
            IBookService service,
            CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Книга с id={id} не найдена" });
        })
        .WithName("DeleteBook")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
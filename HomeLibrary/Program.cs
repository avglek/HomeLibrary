using HomeLibrary.Configuration;
using HomeLibrary.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ── DI: инфраструктура + бизнес-логика + API-сервисы ──
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddApiServices(builder.Configuration);

// Включаем маппинг snake_case → PascalCase для Dapper
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

// ── Middleware ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HomeLibrary API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// ── Эндпоинты ──
app.MapBookEndpoints();

app.Run();

// Требуется для интеграционных тестов через WebApplicationFactory<Program>
public partial class Program { }

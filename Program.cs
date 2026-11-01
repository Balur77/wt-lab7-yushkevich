using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using Scalar.AspNetCore;
using wt_lab7_7postgres_efcore_yushkevich.Data;
using wt_lab7_7postgres_efcore_yushkevich.Models;

var builder = WebApplication.CreateBuilder(args);

// Документация OpenAPI: описание API, теги и относительный адрес сервера.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "CargoFlow API",
            Version = "v1",
            Description = "REST API для учёта грузов на складе. " +
                          "ASP.NET Core Minimal API, Entity Framework Core и PostgreSQL."
        };

        document.Tags = new List<OpenApiTag>
        {
            new() { Name = "General", Description = "Служебные endpoint'ы приложения" },
            new() { Name = "Cargo", Description = "Операции с грузами на складе" },
            new() { Name = "LINQ", Description = "Аналитические запросы через Entity Framework Core" }
        };

        document.Servers = new List<OpenApiServer>
        {
            new() { Url = "/" }
        };

        return Task.CompletedTask;
    });
});

// Строка подключения: локально — ConnectionStrings:DefaultConnection,
// на Render — переменная окружения DATABASE_URL.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    connectionString = BuildConnectionString(databaseUrl);
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Не задана строка подключения. Укажите ConnectionStrings:DefaultConnection " +
        "или переменную окружения DATABASE_URL.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// OpenAPI, Scalar UI и Swagger UI доступны и локально, и на Render.
app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("CargoFlow API — документация"));

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/openapi/v1.json", "CargoFlow API v1");
    options.DocumentTitle = "CargoFlow API — Swagger UI";
});

// Автоматическое применение миграций при запуске.
// Именно Migrate(), а не EnsureCreated(): структура БД создаётся по миграциям.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ApplyMigrations(db);
}

app.MapGet("/", () => "Minimal API работает!")
    .WithName("GetRoot")
    .WithTags("General")
    .WithSummary("Проверить работу приложения");

app.MapGet("/api/info", (AppDbContext db) => Results.Ok(new
{
    Application = "Web Technologies",
    Technology = "ASP.NET Core Minimal API + Entity Framework Core",
    Version = "2.0",
    Domain = "Склад и логистика",
    Entity = "Cargo",
    Storage = "PostgreSQL (EF Core, Code First)",
    Database = db.Database.GetDbConnection().Database,
    Documentation = "/scalar/v1",
    Swagger = "/swagger"
}))
    .WithName("GetInfo")
    .WithTags("General")
    .WithSummary("Получить информацию о приложении и хранилище")
    .Produces(StatusCodes.Status200OK);

// GET /api/cargoes — список с поиском, фильтрацией и сортировкой.
app.MapGet("/api/cargoes", async (
    string? search,
    string? category,
    bool? available,
    decimal? minWeight,
    decimal? maxWeight,
    string? sortBy,
    AppDbContext db) =>
{
    IQueryable<Cargo> query = db.Cargoes.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var pattern = $"%{search.Trim()}%";
        query = query.Where(cargo =>
            EF.Functions.ILike(cargo.Name, pattern) ||
            EF.Functions.ILike(cargo.Category, pattern) ||
            EF.Functions.ILike(cargo.Location, pattern) ||
            EF.Functions.ILike(cargo.Description, pattern) ||
            EF.Functions.ILike(cargo.Supplier, pattern));
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        var categoryFilter = category.Trim();
        query = query.Where(cargo => cargo.Category == categoryFilter);
    }

    if (available is not null)
    {
        query = query.Where(cargo => cargo.IsAvailable == available);
    }

    if (minWeight is not null)
    {
        query = query.Where(cargo => cargo.Weight >= minWeight);
    }

    if (maxWeight is not null)
    {
        query = query.Where(cargo => cargo.Weight <= maxWeight);
    }

    query = sortBy?.ToLowerInvariant() switch
    {
        "name" => query.OrderBy(cargo => cargo.Name),
        "category" => query.OrderBy(cargo => cargo.Category).ThenBy(cargo => cargo.Name),
        "weight" => query.OrderByDescending(cargo => cargo.Weight),
        "quantity" => query.OrderByDescending(cargo => cargo.Quantity),
        _ => query.OrderBy(cargo => cargo.Id)
    };

    var cargoes = await query.ToListAsync();
    return Results.Ok(cargoes);
})
    .WithName("GetCargoes")
    .WithTags("Cargo")
    .WithSummary("Получить список грузов")
    .WithDescription("Поддерживает поиск, фильтрацию по категории, наличию и весу, " +
                     "а также сортировку по названию, категории, весу и количеству.")
    .Produces<IEnumerable<Cargo>>(StatusCodes.Status200OK);

// GET /api/cargoes/search — отдельный поисковый endpoint.
app.MapGet("/api/cargoes/search", async (string? search, AppDbContext db) =>
{
    var pattern = $"%{search?.Trim() ?? string.Empty}%";
    var cargoes = await db.Cargoes
        .AsNoTracking()
        .Where(cargo => EF.Functions.ILike(cargo.Name, pattern) ||
                        EF.Functions.ILike(cargo.Category, pattern) ||
                        EF.Functions.ILike(cargo.Description, pattern) ||
                        EF.Functions.ILike(cargo.Supplier, pattern))
        .OrderBy(cargo => cargo.Name)
        .ToListAsync();

    return Results.Ok(cargoes);
})
    .WithName("SearchCargoes")
    .WithTags("Cargo")
    .WithSummary("Найти грузы по названию, категории или описанию")
    .Produces<IEnumerable<Cargo>>(StatusCodes.Status200OK);

// GET /api/cargoes/available — LINQ-запрос: WHERE + ORDER BY.
app.MapGet("/api/cargoes/available", async (AppDbContext db) =>
{
    var cargoes = await db.Cargoes
        .AsNoTracking()
        .Where(cargo => cargo.IsAvailable)
        .OrderBy(cargo => cargo.Name)
        .ToListAsync();

    return Results.Ok(cargoes);
})
    .WithName("GetAvailableCargoes")
    .WithTags("LINQ")
    .WithSummary("Получить доступные грузы, отсортированные по названию")
    .Produces<IEnumerable<Cargo>>(StatusCodes.Status200OK);

// GET /api/cargoes/heavy?minWeight=10 — LINQ-запрос с фильтрацией по decimal.
app.MapGet("/api/cargoes/heavy", async (decimal? minWeight, AppDbContext db) =>
{
    var threshold = minWeight ?? 10m;
    var cargoes = await db.Cargoes
        .AsNoTracking()
        .Where(cargo => cargo.Weight >= threshold)
        .OrderByDescending(cargo => cargo.Weight)
        .ToListAsync();

    return Results.Ok(new
    {
        MinWeight = threshold,
        Count = cargoes.Count,
        Items = cargoes
    });
})
    .WithName("GetHeavyCargoes")
    .WithTags("LINQ")
    .WithSummary("Получить грузы с весом не меньше заданного")
    .Produces(StatusCodes.Status200OK);

// GET /api/cargoes/stats — LINQ-запрос с GroupBy и агрегатными функциями.
app.MapGet("/api/cargoes/stats", async (AppDbContext db) =>
{
    var stats = await db.Cargoes
        .AsNoTracking()
        .GroupBy(cargo => cargo.Category)
        .Select(group => new
        {
            Category = group.Key,
            Count = group.Count(),
            TotalQuantity = group.Sum(cargo => cargo.Quantity),
            TotalWeight = group.Sum(cargo => cargo.Weight * cargo.Quantity),
            AverageWeight = group.Average(cargo => cargo.Weight),
            AvailableCount = group.Count(cargo => cargo.IsAvailable)
        })
        .OrderByDescending(item => item.TotalWeight)
        .ToListAsync();

    return Results.Ok(stats);
})
    .WithName("GetCargoStats")
    .WithTags("LINQ")
    .WithSummary("Статистика по категориям: количество, вес и наличие")
    .Produces(StatusCodes.Status200OK);

// GET /api/cargoes/recent?days=30 — LINQ-запрос по дате добавления.
app.MapGet("/api/cargoes/recent", async (int? days, AppDbContext db) =>
{
    var period = days is > 0 ? days.Value : 30;
    var since = DateTime.UtcNow.AddDays(-period);

    var cargoes = await db.Cargoes
        .AsNoTracking()
        .Where(cargo => cargo.CreatedAt >= since)
        .OrderByDescending(cargo => cargo.CreatedAt)
        .ToListAsync();

    return Results.Ok(new
    {
        Days = period,
        Since = since,
        Count = cargoes.Count,
        Items = cargoes
    });
})
    .WithName("GetRecentCargoes")
    .WithTags("LINQ")
    .WithSummary("Получить грузы, добавленные за последние N дней")
    .Produces(StatusCodes.Status200OK);

// GET /api/cargoes/{id} — получение объекта по идентификатору.
app.MapGet("/api/cargoes/{id:int}", async (int id, AppDbContext db) =>
{
    var cargo = await db.Cargoes
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.Id == id);

    return cargo is null
        ? Results.NotFound()
        : Results.Ok(cargo);
})
    .WithName("GetCargoById")
    .WithTags("Cargo")
    .WithSummary("Получить груз по идентификатору")
    .Produces<Cargo>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// POST /api/cargoes — создание груза.
app.MapPost("/api/cargoes", async (Cargo input, AppDbContext db) =>
{
    Normalize(input);

    var errors = Validate(input);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    input.Id = 0;
    input.CreatedAt = DateTime.UtcNow;
    db.Cargoes.Add(input);
    await db.SaveChangesAsync();

    return Results.Created($"/api/cargoes/{input.Id}", input);
})
    .WithName("CreateCargo")
    .WithTags("Cargo")
    .WithSummary("Создать груз")
    .Accepts<Cargo>("application/json")
    .Produces<Cargo>(StatusCodes.Status201Created)
    .ProducesValidationProblem();

// PUT /api/cargoes/{id} — изменение груза.
app.MapPut("/api/cargoes/{id:int}", async (int id, Cargo input, AppDbContext db) =>
{
    Normalize(input);

    var cargo = await db.Cargoes.FirstOrDefaultAsync(item => item.Id == id);
    if (cargo is null)
    {
        return Results.NotFound();
    }

    var errors = Validate(input);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    cargo.Name = input.Name;
    cargo.Category = input.Category;
    cargo.Quantity = input.Quantity;
    cargo.Location = input.Location;
    cargo.Weight = input.Weight;
    cargo.Description = input.Description;
    cargo.IsAvailable = input.IsAvailable;
    cargo.Supplier = input.Supplier;

    await db.SaveChangesAsync();

    return Results.Ok(cargo);
})
    .WithName("UpdateCargo")
    .WithTags("Cargo")
    .WithSummary("Изменить груз")
    .Accepts<Cargo>("application/json")
    .Produces<Cargo>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

// DELETE /api/cargoes/{id} — удаление груза.
app.MapDelete("/api/cargoes/{id:int}", async (int id, AppDbContext db) =>
{
    var cargo = await db.Cargoes.FirstOrDefaultAsync(item => item.Id == id);
    if (cargo is null)
    {
        return Results.NotFound();
    }

    db.Cargoes.Remove(cargo);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
    .WithName("DeleteCargo")
    .WithTags("Cargo")
    .WithSummary("Удалить груз")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

// Render передаёт порт в переменной PORT; приложение слушает 0.0.0.0.
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");

/// <summary>Применяет миграции с несколькими попытками: база на Render может стартовать позже.</summary>
static void ApplyMigrations(AppDbContext db)
{
    const int maxAttempts = 10;

    for (var attempt = 1; ; attempt++)
    {
        try
        {
            db.Database.Migrate();
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            Console.WriteLine(
                $"Не удалось подключиться к PostgreSQL (попытка {attempt}/{maxAttempts}): " +
                $"{exception.Message}. Повтор через 3 секунды.");
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}

/// <summary>
/// Преобразует DATABASE_URL Render формата postgres://user:password@host:port/database
/// в строку подключения Npgsql. Пароль не попадает в исходный код и репозиторий.
/// </summary>
static string BuildConnectionString(string databaseUrl)
{
    if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        // Уже готовая строка подключения Npgsql.
        return databaseUrl;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var connectionBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty
    };

    var sslModeValue = Environment.GetEnvironmentVariable("DATABASE_SSL_MODE");
    if (!string.IsNullOrWhiteSpace(sslModeValue) &&
        Enum.TryParse<SslMode>(sslModeValue, ignoreCase: true, out var sslMode))
    {
        connectionBuilder.SslMode = sslMode;
    }
    else
    {
        // Внутренний PostgreSQL Render и локальный контейнер доступны без TLS.
        // Внешний адрес Render (*.render.com) требует TLS.
        connectionBuilder.SslMode = uri.Host.EndsWith(".render.com", StringComparison.OrdinalIgnoreCase)
            ? SslMode.Require
            : SslMode.Disable;
    }

    return connectionBuilder.ConnectionString;
}

/// <summary>Удаляет лишние пробелы в строковых полях перед сохранением.</summary>
static void Normalize(Cargo cargo)
{
    cargo.Name = (cargo.Name ?? string.Empty).Trim();
    cargo.Category = (cargo.Category ?? string.Empty).Trim();
    cargo.Location = (cargo.Location ?? string.Empty).Trim();
    cargo.Description = (cargo.Description ?? string.Empty).Trim();
    cargo.Supplier = (cargo.Supplier ?? string.Empty).Trim();
}

/// <summary>
/// Проверяет модель по правилам DataAnnotations и возвращает ошибки
/// в формате, подходящем для Results.ValidationProblem (400 Bad Request).
/// </summary>
static Dictionary<string, string[]> Validate(Cargo cargo)
{
    var results = new List<ValidationResult>();
    var context = new ValidationContext(cargo);

    if (Validator.TryValidateObject(cargo, context, results, validateAllProperties: true))
    {
        return new Dictionary<string, string[]>();
    }

    return results
        .SelectMany(result =>
        {
            var members = result.MemberNames.Any()
                ? result.MemberNames
                : new[] { "cargo" };

            return members.Select(member => new
            {
                Member = member,
                Message = result.ErrorMessage ?? "Некорректное значение."
            });
        })
        .GroupBy(item => item.Member)
        .ToDictionary(
            group => group.Key,
            group => group.Select(item => item.Message).ToArray());
}

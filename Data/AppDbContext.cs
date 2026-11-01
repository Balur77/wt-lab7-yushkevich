using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_yushkevich.Models;

namespace wt_lab7_7postgres_efcore_yushkevich.Data;

/// <summary>
/// Контекст Entity Framework Core — точка взаимодействия приложения
/// с моделью данных и базой PostgreSQL.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cargo> Cargoes => Set<Cargo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.ToTable("Cargoes");
            entity.HasKey(cargo => cargo.Id);

            entity.Property(cargo => cargo.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(cargo => cargo.Category)
                .HasMaxLength(60)
                .IsRequired();

            entity.Property(cargo => cargo.Location)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(cargo => cargo.Description)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(cargo => cargo.Weight)
                .HasPrecision(18, 3);

            entity.Property(cargo => cargo.CreatedAt)
                .IsRequired();

            entity.Property(cargo => cargo.Supplier)
                .HasMaxLength(120)
                .IsRequired();

            entity.HasIndex(cargo => cargo.Category);
            entity.HasIndex(cargo => cargo.IsAvailable);
            entity.HasIndex(cargo => cargo.CreatedAt);

            // Начальные данные создаются миграцией SeedCargoes,
            // а не вручную через SQL-клиент.
            entity.HasData(
                new Cargo
                {
                    Id = 1,
                    Name = "Ноутбуки Dell Latitude 5440",
                    Category = "Электроника",
                    Quantity = 24,
                    Location = "Сектор A-1, стеллаж 3",
                    Weight = 2.4m,
                    Description = "Партия офисных ноутбуков для оснащения новых рабочих мест. Гарантия производителя — 24 месяца.",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc),
                    Supplier = "Dell Technologies"
                },
                new Cargo
                {
                    Id = 2,
                    Name = "Офисные кресла ErgoPro",
                    Category = "Мебель",
                    Quantity = 18,
                    Location = "Сектор B-2, зона сборки",
                    Weight = 12.5m,
                    Description = "Эргономичные кресла с регулируемой поддержкой поясницы. Поставка в разобранном виде.",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2026, 1, 12, 10, 30, 0, DateTimeKind.Utc),
                    Supplier = "ErgoPro"
                },
                new Cargo
                {
                    Id = 3,
                    Name = "Кофемашины Jura E8",
                    Category = "Бытовая техника",
                    Quantity = 6,
                    Location = "Сектор C-1, стеллаж 1",
                    Weight = 9.8m,
                    Description = "Профессиональные кофемашины для переговорных комнат и клиентских зон.",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2026, 1, 15, 8, 15, 0, DateTimeKind.Utc),
                    Supplier = "Jura"
                },
                new Cargo
                {
                    Id = 4,
                    Name = "Наборы строительного инструмента",
                    Category = "Инструменты",
                    Quantity = 45,
                    Location = "Сектор D-3, паллета 12",
                    Weight = 25m,
                    Description = "Универсальные наборы инструментов для ремонтных бригад: дрели, шуруповёрты и расходники.",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2026, 1, 18, 12, 0, 0, DateTimeKind.Utc),
                    Supplier = "ИнструментТорг"
                },
                new Cargo
                {
                    Id = 5,
                    Name = "Медицинские маски",
                    Category = "Медицина",
                    Quantity = 1200,
                    Location = "Сектор E-2, стеллаж 5",
                    Weight = 0.005m,
                    Description = "Одноразовые трёхслойные маски в индивидуальной упаковке. Поставка коробами по 50 штук.",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc),
                    Supplier = "МедСнаб"
                },
                new Cargo
                {
                    Id = 6,
                    Name = "Серверное оборудование Supermicro",
                    Category = "Электроника",
                    Quantity = 4,
                    Location = "Сектор A-3, климат-зона",
                    Weight = 18.6m,
                    Description = "Стоечные серверы для модернизации дата-центра. Ожидают таможенного оформления.",
                    IsAvailable = false,
                    CreatedAt = new DateTime(2026, 1, 22, 16, 20, 0, DateTimeKind.Utc),
                    Supplier = "Supermicro"
                });
        });
    }
}

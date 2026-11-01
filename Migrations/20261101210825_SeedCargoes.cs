using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wt_lab7_7postgres_efcore_yushkevich.Migrations
{
    /// <inheritdoc />
    public partial class SeedCargoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cargoes",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "IsAvailable", "Location", "Name", "Quantity", "Weight" },
                values: new object[,]
                {
                    { 1, "Электроника", new DateTime(2026, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), "Партия офисных ноутбуков для оснащения новых рабочих мест. Гарантия производителя — 24 месяца.", true, "Сектор A-1, стеллаж 3", "Ноутбуки Dell Latitude 5440", 24, 2.4m },
                    { 2, "Мебель", new DateTime(2026, 1, 12, 10, 30, 0, 0, DateTimeKind.Utc), "Эргономичные кресла с регулируемой поддержкой поясницы. Поставка в разобранном виде.", true, "Сектор B-2, зона сборки", "Офисные кресла ErgoPro", 18, 12.5m },
                    { 3, "Бытовая техника", new DateTime(2026, 1, 15, 8, 15, 0, 0, DateTimeKind.Utc), "Профессиональные кофемашины для переговорных комнат и клиентских зон.", true, "Сектор C-1, стеллаж 1", "Кофемашины Jura E8", 6, 9.8m },
                    { 4, "Инструменты", new DateTime(2026, 1, 18, 12, 0, 0, 0, DateTimeKind.Utc), "Универсальные наборы инструментов для ремонтных бригад: дрели, шуруповёрты и расходники.", true, "Сектор D-3, паллета 12", "Наборы строительного инструмента", 45, 25m },
                    { 5, "Медицина", new DateTime(2026, 1, 20, 14, 45, 0, 0, DateTimeKind.Utc), "Одноразовые трёхслойные маски в индивидуальной упаковке. Поставка коробами по 50 штук.", true, "Сектор E-2, стеллаж 5", "Медицинские маски", 1200, 0.005m },
                    { 6, "Электроника", new DateTime(2026, 1, 22, 16, 20, 0, 0, DateTimeKind.Utc), "Стоечные серверы для модернизации дата-центра. Ожидают таможенного оформления.", false, "Сектор A-3, климат-зона", "Серверное оборудование Supermicro", 4, 18.6m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}

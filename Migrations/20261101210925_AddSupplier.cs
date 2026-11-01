using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wt_lab7_7postgres_efcore_yushkevich.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "Cargoes",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Supplier",
                value: "Dell Technologies");

            migrationBuilder.UpdateData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Supplier",
                value: "ErgoPro");

            migrationBuilder.UpdateData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Supplier",
                value: "Jura");

            migrationBuilder.UpdateData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Supplier",
                value: "ИнструментТорг");

            migrationBuilder.UpdateData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Supplier",
                value: "МедСнаб");

            migrationBuilder.UpdateData(
                table: "Cargoes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Supplier",
                value: "Supermicro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "Cargoes");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasker.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NumeradorInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Numerador",
                columns: new[] { "Id", "Tabla", "UltimoNumero" },
                values: new object[] { 1, "Factura", 5913L });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Numerador",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}

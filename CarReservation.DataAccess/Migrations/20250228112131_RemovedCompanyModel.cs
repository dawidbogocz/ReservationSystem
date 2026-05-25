using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedCompanyModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Company_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Car_Company_CompanyId",
                table: "Car");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Car_CompanyId",
                table: "Car");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Car");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7b14c511-1601-416e-930b-4510ee295089", "ec87b89b-d064-48a0-936c-d3b664118f67" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "016a07d9-620c-4995-9f2e-4ef5664326eb", "abb7d148-3e4d-4963-825e-41adbe8be141" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "8d6e83cf-f006-43de-b962-b879b63acac1", "ad0b6055-eb9f-4bdf-9621-1d64454353c5" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateReported",
                value: new DateTime(2025, 2, 28, 12, 21, 30, 370, DateTimeKind.Local).AddTicks(6304));

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateReported",
                value: new DateTime(2025, 2, 28, 12, 21, 30, 370, DateTimeKind.Local).AddTicks(6309));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Car",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "CompanyId", "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { 1, "02e55a76-f12d-4b32-b40f-1d31c1fec350", "6b72bd97-02e8-4bc8-bc4e-87b3a1cd00ff" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "CompanyId", "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { 2, "c5c17291-c110-495e-b5c5-33e0a64ed992", "bdc082c6-97a2-4938-8c76-c83e7ffb42b9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "CompanyId", "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { 1, "1ffe8ac5-2510-4123-8dba-ab1f3969620f", "82d324f1-5045-4340-a8e8-dd87c95c54bc" });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "ABC123",
                column: "CompanyId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "DEF456",
                column: "CompanyId",
                value: null);

            migrationBuilder.InsertData(
                table: "Company",
                columns: new[] { "Id", "Location", "NIP", "Name" },
                values: new object[,]
                {
                    { 1, "Adres1", "1234567890", "Firma1" },
                    { 2, "Adres2", "0987654321", "Firma2" }
                });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateReported",
                value: new DateTime(2025, 2, 7, 8, 39, 36, 184, DateTimeKind.Local).AddTicks(27));

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateReported",
                value: new DateTime(2025, 2, 7, 8, 39, 36, 184, DateTimeKind.Local).AddTicks(32));

            migrationBuilder.CreateIndex(
                name: "IX_Car_CompanyId",
                table: "Car",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Company_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Car_Company_CompanyId",
                table: "Car",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FixDescription",
                table: "Fault",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "c34ba83d-5257-4cb2-9a5a-a5711b9e26fb", "94029373-4854-40cc-8b01-a9e0558d81d6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "5a360d64-8349-43ba-b732-f7ea96237b70", "190f7df6-4fa2-4cb1-9388-460f9bfbd7c1" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ce520ae1-fdaa-4808-b8b9-f9ed80b6e998", "eb5f3101-db73-4077-ac62-f23e5a3838c3" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "FixDescription",
                value: null);

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "FixDescription",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixDescription",
                table: "Fault");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7e47e28b-be72-4c9a-a8bc-882b97850070", "9982438d-9f21-4ed2-a87a-4d49a2e1c2d5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "601f6f0b-0cca-4e13-ad99-ce022a74635d", "5d6dfe64-51bc-4341-b41a-80d65201397a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f6c1a1c4-2b5f-4234-b9e8-8557f66ac87c", "053706fe-8c11-4803-a5da-9767b818085b" });
        }
    }
}

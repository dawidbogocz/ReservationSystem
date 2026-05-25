using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ExternalLoginModelExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "02e55a76-f12d-4b32-b40f-1d31c1fec350", "6b72bd97-02e8-4bc8-bc4e-87b3a1cd00ff" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "c5c17291-c110-495e-b5c5-33e0a64ed992", "bdc082c6-97a2-4938-8c76-c83e7ffb42b9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "1ffe8ac5-2510-4123-8dba-ab1f3969620f", "82d324f1-5045-4340-a8e8-dd87c95c54bc" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "b8cedaad-b7a5-4cca-b95a-077533e34429", "7add2859-42c9-4742-b60a-efc8e2936f55" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2b616add-790f-4aa1-a387-7c5fa2fc1af9", "68dc2fdf-d5a8-4861-8fd9-220638fcec4e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "57735de9-7d12-4a85-804c-2edc9ddeeec8", "86e71338-fd53-45b5-9477-389f24ee5e43" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateReported",
                value: new DateTime(2025, 1, 24, 10, 19, 30, 105, DateTimeKind.Local).AddTicks(5384));

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateReported",
                value: new DateTime(2025, 1, 24, 10, 19, 30, 105, DateTimeKind.Local).AddTicks(5389));
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EmailFirstNameLastNameUniqueId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4eda41a6-9c1a-4d2d-90dd-56f9dd2113ce", "98d5be9f-0be3-445f-be1c-5c3c5fcdefbe" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "bcaaffca-128b-449d-9c36-e02f14dea162", "39341b07-695b-44d1-8f15-8b51fb3d29ce" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f2e8aefa-6bc1-4c95-86a7-0bb9c4feffc3", "7e7278cc-be77-4830-bcee-b4849b1bd4af" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email_FirstName_LastName",
                table: "AspNetUsers",
                columns: new[] { "Email", "FirstName", "LastName" },
                unique: true,
                filter: "[Email] IS NOT NULL AND [FirstName] IS NOT NULL AND [LastName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email_FirstName_LastName",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3e40146e-9224-4b07-91ab-5f8501133cf7", "2979a37e-d32a-44b2-b348-927b923f4146" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f1b89359-d306-4fff-898b-94e1cf3d78a2", "649cedbf-25fe-45b8-8e7c-80fef6926552" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7db9ecc8-dd7b-4a98-9630-6b0bf0296c53", "ea355d5d-92d6-4969-bcd3-e374b0fea801" });
        }
    }
}

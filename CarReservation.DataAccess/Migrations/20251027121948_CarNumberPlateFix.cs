using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CarNumberPlateFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CarNumberPlate",
                table: "FeedbackLogs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6b946012-54fa-4814-a65c-454480a2ee3a", "f5abe40d-cb4e-4681-907b-633d165aad31" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ee50819c-ee7d-4a14-98fa-0faa381273e7", "ba4854c5-14e5-4b7d-a8e9-e2aa406e5bdd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "70236d11-b75d-405a-94e1-b054838191fc", "fd636751-d184-4264-a5fd-5b4779125d80" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CarNumberPlate",
                table: "FeedbackLogs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f8f5369b-5950-487a-81f5-dd4a4bda5c52", "ce1846c2-3ea7-4fcb-b71e-88ffb1403338" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "5f171e11-1dff-49bf-8614-6468f99f5ebd", "e38b307b-3871-41c8-bd60-392514960c98" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "0ec8449f-c818-4ec1-8564-0babb0fac7da", "ed29889e-49f9-4524-8e18-aea9a2fe170c" });
        }
    }
}

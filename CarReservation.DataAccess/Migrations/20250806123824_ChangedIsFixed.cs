using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedIsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "c295fdb2-4a2e-4d6f-be96-f6d9226bd657", "04fc1b74-0988-46be-8ece-550a54a8d6cd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "d9aaa18b-b313-4dc9-99bc-80ec6445c8bf", "67080f1f-8f27-467f-ae02-f8e8d7a2b65b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "27a61270-27e3-4bb2-ac38-f7c2d2682a09", "d1ca4840-1f8e-454d-88b5-8f2d18fbf17b" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsDrivable",
                value: false);

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsDrivable",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "47229aa3-ede7-4b0f-be68-159a1fef24a8", "24775c9e-1c16-4ba1-af0c-4a09ee1c3945" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "04e327f8-3d58-47d1-8603-00f6ca9f05e5", "3b1edf00-4ccd-41a7-b2b7-92d4a3e2ce98" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "affa91fc-6f9e-49bf-9257-38fad226f325", "8a329616-5400-4b76-96df-251b6bb1fb94" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsDrivable",
                value: true);

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsDrivable",
                value: true);
        }
    }
}

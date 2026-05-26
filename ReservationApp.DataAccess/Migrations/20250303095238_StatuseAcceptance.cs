using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class StatuseAcceptance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptStatute",
                table: "Reservation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "31f84bfb-3845-4931-9c05-a551d82af80e", "850774f0-3e14-4478-b49a-4e70ebf0d891" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "d8e17895-b501-48a1-b3d9-a8a30aa1e6b4", "a99a7573-1389-451c-92f7-0017f42a295f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a634d87e-b4f8-44a2-a747-2d3f6a49fd40", "d3dbe0d4-b087-4571-8e52-14d4bcb3b4b6" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateReported",
                value: new DateTime(2025, 3, 3, 10, 52, 36, 837, DateTimeKind.Local).AddTicks(8463));

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateReported",
                value: new DateTime(2025, 3, 3, 10, 52, 36, 837, DateTimeKind.Local).AddTicks(8468));

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 1,
                column: "AcceptStatute",
                value: false);

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 2,
                column: "AcceptStatute",
                value: false);

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 3,
                column: "AcceptStatute",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptStatute",
                table: "Reservation");

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
    }
}

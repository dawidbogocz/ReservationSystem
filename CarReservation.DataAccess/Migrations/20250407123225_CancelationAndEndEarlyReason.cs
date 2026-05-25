using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CancelationAndEndEarlyReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Reservation",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a0183101-8c13-4429-81a5-c03fc8de30d7", "2c90291f-a2a8-4633-9678-3a59968a8983" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4f198bc9-3226-4136-bfc1-7228c355da27", "893e04f2-fcae-49d6-921b-86eaf5118cbc" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "90c19fc6-d977-48c6-a985-c7f2e2bde6c0", "29469d12-cf7d-46e1-ba6b-fea5c1a3d91c" });

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 1,
                column: "Note",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 2,
                column: "Note",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 3,
                column: "Note",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Reservation");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "efc0c52d-deb5-4aa3-97d0-b089839b2c0c", "0bf01ab2-c830-468f-9422-72a2741e5049" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "cc5886a6-3746-4e32-bd9b-c62c8a88a233", "b54e3e2f-c540-40a2-85dd-ffd78fae0e45" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "b073f680-e394-4437-aaf1-e1951d1aff23", "2d0cda45-5537-4d26-ba37-bda1ee5500e4" });
        }
    }
}

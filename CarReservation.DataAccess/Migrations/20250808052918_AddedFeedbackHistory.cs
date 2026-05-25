using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedFeedbackHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PickupMileage",
                table: "Reservation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FeedbackLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    CarNumberPlate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: true),
                    FuelLevel = table.Column<int>(type: "int", nullable: true),
                    IsCarDirty = table.Column<bool>(type: "bit", nullable: true),
                    HasFaults = table.Column<bool>(type: "bit", nullable: true),
                    Faults = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FeedbackLogs_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "72d8250b-b306-4996-ad43-d82f8ba28645", "cc6ccd9b-3159-4767-bd2c-bdf40631e1d3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "8d6410b2-e483-4451-99c0-31681307a684", "509e4e4e-23e2-4f25-aa52-9e9eba9f1a37" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "e64c41d9-f4a9-436e-9ee9-6a1022882dd5", "d4e289ef-bee6-4354-8e14-2d18aae93be4" });

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 1,
                column: "PickupMileage",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 2,
                column: "PickupMileage",
                value: null);

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 3,
                column: "PickupMileage",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackLogs_ReservationId",
                table: "FeedbackLogs",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackLogs_UserId",
                table: "FeedbackLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackLogs");

            migrationBuilder.DropColumn(
                name: "PickupMileage",
                table: "Reservation");

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
        }
    }
}

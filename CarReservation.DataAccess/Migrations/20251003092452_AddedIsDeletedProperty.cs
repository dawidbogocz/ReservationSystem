using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsDeletedProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackLogs_AspNetUsers_UserId",
                table: "FeedbackLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackLogs_Reservation_ReservationId",
                table: "FeedbackLogs");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Car",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2a859d96-3cfc-4fd1-b8d8-757f42f66f78", "bb8e937f-5918-4162-9632-1777a12c3e99" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6d430517-b1aa-4a87-8f7b-52cfc21a038f", "cbcc5b72-ad5f-49a0-a403-54dda9084c1d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "31ce31ff-1b28-4b9d-84eb-97ef94901108", "97f949f8-e578-4c4e-aa45-6088368c9f4c" });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "ABC123",
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "DEF456",
                column: "IsDeleted",
                value: false);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackLogs_AspNetUsers_UserId",
                table: "FeedbackLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackLogs_Reservation_ReservationId",
                table: "FeedbackLogs",
                column: "ReservationId",
                principalTable: "Reservation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackLogs_AspNetUsers_UserId",
                table: "FeedbackLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackLogs_Reservation_ReservationId",
                table: "FeedbackLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Car");

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

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackLogs_AspNetUsers_UserId",
                table: "FeedbackLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackLogs_Reservation_ReservationId",
                table: "FeedbackLogs",
                column: "ReservationId",
                principalTable: "Reservation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
